import { Activity } from "../abstracts/Activity";

import { ContainerActivity } from "../abstracts/ContainerActivity";
import { Flowchart } from "../activities/Flowchart";
import { ActivityConnection } from "../models/ActivityConnection";
import { ActivityEndpoint } from "../models/ActivityEndpoint";
import { resolveActivityClass } from "./registry";

/**
 * Wire-format (de)serialization between the backend activity-tree JSON and the
 * frontend mirrored model classes.
 *
 * Fidelity rules:
 * - Known base fields map onto the Activity base class.
 * - Keys the concrete class declares are assigned as-is (nested activities and
 *   Input-shaped values are plain objects after load; behaviors that need them
 *   — ports, projection — work off plain data).
 * - Keys the class does not declare are preserved in an extra bag and merged
 *   back on serialize, so a roundtrip never drops backend fields.
 * - Unknown activity types become GenericActivity, which keeps the entire raw
 *   JSON and is serialized back verbatim (with base fields overridden).
 */

export type WireActivity = {
  type?: string;
  id?: string;
  [key: string]: unknown;
};

const BASE_FIELDS = new Set([
  "id",
  "nodeId",
  "code",
  "name",
  "type",
  "version",
  "customProperties",
  "metadata",
]);

/** Unmodeled wire keys, preserved per activity across a roundtrip. */
const extraStore = new WeakMap<Activity, Record<string, unknown>>();

export class GenericActivity extends Activity {
  /** The original wire JSON, serialized back verbatim. */
  raw: Record<string, unknown>;
  /** Hydrated children when the raw JSON is container-like. */
  activities: Activity[] = [];
  connections: ActivityConnection[] = [];

  constructor(type: string, raw: Record<string, unknown>) {
    super();
    this.type = type;
    this.raw = raw;
  }

  override getOutPorts() {
    // Unknown activities render with a single default outcome so they can
    // still participate in the canvas execution flow.
    return [{ name: "Done", displayName: "Done" }];
  }
}

function isWireActivity(value: unknown): value is WireActivity {
  return (
    typeof value === "object" &&
    value !== null &&
    !Array.isArray(value) &&
    typeof (value as WireActivity).type === "string"
  );
}

function fromWireConnections(raw: unknown): ActivityConnection[] {
  if (!Array.isArray(raw)) return [];
  return raw.map((item) => {
    const source = (item as { source?: { activityId?: unknown; port?: unknown } })?.source;
    const target = (item as { target?: { activityId?: unknown; port?: unknown } })?.target;
    return new ActivityConnection(
      new ActivityEndpoint(
        String(source?.activityId ?? ""),
        source?.port != null ? String(source.port) : undefined,
      ),
      new ActivityEndpoint(
        String(target?.activityId ?? ""),
        target?.port != null ? String(target.port) : undefined,
      ),
    );
  });
}

function toWireConnection(connection: ActivityConnection) {
  return {
    source: { activityId: connection.source.activityId, ...(connection.source.port ? { port: connection.source.port } : {}) },
    target: { activityId: connection.target.activityId, ...(connection.target.port ? { port: connection.target.port } : {}) },
  };
}

export function fromWireActivity(json: WireActivity): Activity {
  const type = typeof json.type === "string" && json.type ? json.type : "Cike.Unknown";
  const Ctor = resolveActivityClass(type);
  const activity: Activity = Ctor ? new Ctor() : new GenericActivity(type, { ...json });

  if (json.id != null) activity.id = String(json.id);
  activity.nodeId = json.nodeId != null ? String(json.nodeId) : null;
  if (json.code != null) activity.code = String(json.code);
  if (json.name != null) activity.name = String(json.name);
  if (json.version != null) activity.version = Number(json.version);
  activity.customProperties = (json.customProperties as Record<string, unknown>) ?? {};
  activity.metadata = (json.metadata as Record<string, unknown>) ?? {};

  const extra: Record<string, unknown> = {};
  const declaredFields = new Set(Object.keys(activity));
  const handledKeys = new Set<string>(["type", "activities", "connections"]);

  for (const [key, value] of Object.entries(json)) {
    if (BASE_FIELDS.has(key) || handledKeys.has(key)) continue;
    if (isWireActivity(value)) {
      Object.assign(activity, { [key]: fromWireActivity(value) });
      continue;
    }
    if (declaredFields.has(key)) {
      Object.assign(activity, { [key]: value });
      continue;
    }
    extra[key] = value;
  }
  extraStore.set(activity, extra);

  if (activity instanceof ContainerActivity) {
    const wireChildren = Array.isArray(json.activities) ? json.activities : [];
    activity.activities = wireChildren.map((child) => fromWireActivity(child as WireActivity));
  }
  if (activity instanceof Flowchart) {
    activity.connections = fromWireConnections(json.connections);
  }
  if (activity instanceof GenericActivity) {
    activity.activities = (Array.isArray(json.activities) ? json.activities : []).map((child) =>
      fromWireActivity(child as WireActivity),
    );
    activity.connections = fromWireConnections(json.connections);
  }

  return activity;
}

function serializeValue(value: unknown): unknown {
  if (value == null) return null;
  if (value instanceof Activity) return toWireActivity(value);
  if (Array.isArray(value)) return value.map(serializeValue);
  if (value instanceof Date) return value.toISOString();
  if (typeof value === "object") {
    const out: Record<string, unknown> = {};
    for (const [key, entry] of Object.entries(value)) out[key] = serializeValue(entry);
    return out;
  }
  return value;
}

function baseWire(activity: Activity): Record<string, unknown> {
  const wire: Record<string, unknown> = { id: activity.id, type: activity.type };
  if (activity.nodeId) wire.nodeId = activity.nodeId;
  if (activity.code) wire.code = activity.code;
  if (activity.name != null) wire.name = activity.name;
  if (activity.version != null) wire.version = activity.version;
  if (activity.customProperties && Object.keys(activity.customProperties).length > 0)
    wire.customProperties = activity.customProperties;
  if (activity.metadata && Object.keys(activity.metadata).length > 0) wire.metadata = activity.metadata;
  return wire;
}

function toWireKnown(activity: Activity): Record<string, unknown> {
  const wire = baseWire(activity);
  const skip = new Set<string>([...BASE_FIELDS, "extra", "raw", "activities", "connections"]);
  for (const [key, value] of Object.entries(activity)) {
    if (skip.has(key) || typeof value === "function") continue;
    wire[key] = serializeValue(value);
  }
  if (activity instanceof ContainerActivity) {
    wire.activities = activity.activities.map((child) => toWireActivity(child as Activity));
  }
  if (activity instanceof Flowchart) {
    wire.connections = activity.connections.map((connection) => toWireConnection(connection));
  }
  return wire;
}

function toWireGeneric(activity: GenericActivity): Record<string, unknown> {
  const wire: Record<string, unknown> = { ...activity.raw, ...baseWire(activity) };
  if (activity.activities.length > 0) wire.activities = activity.activities.map((child) => toWireActivity(child));
  if (activity.connections.length > 0) wire.connections = activity.connections.map((c) => toWireConnection(c));
  // Designer mutations (e.g. positions) may land on metadata even for unknown types.
  if (activity.metadata && Object.keys(activity.metadata).length > 0) wire.metadata = activity.metadata;
  return wire;
}

export function toWireActivity(activity: Activity): Record<string, unknown> {
  if (activity instanceof GenericActivity) return toWireGeneric(activity);
  const wire = toWireKnown(activity);
  const extra = extraStore.get(activity);
  // Extra keys go first so explicitly modeled fields always win.
  return extra ? { ...extra, ...wire } : wire;
}
