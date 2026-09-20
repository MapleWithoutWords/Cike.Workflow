import { Activity } from "../abstracts/Activity";
import { Flowchart } from "../activities/Flowchart";
import { GenericActivity } from "./serialization";

/**
 * Resolve the container to display when drilling into `activity`.
 * - Flowchart nodes drill into themselves.
 * - Property-held bodies (ForEach/While/For `body`) drill into the body; a null
 *   body gets an empty Flowchart created (spec ticket: drill navigation). A
 *   non-mirrored body cannot be edited, so it is not drillable (null).
 * - Anything else is not drillable.
 */
export function ensureDrillTarget(activity: Activity): Activity | null {
  if (activity instanceof Flowchart) return activity;
  const holder = activity as { body?: unknown; activities?: unknown };
  if ("body" in holder) {
    const body = holder.body;
    if (body == null) {
      const created = new Flowchart();
      holder.body = created;
      return created;
    }
    if (body instanceof Activity && !(body instanceof GenericActivity)) return body;
    return null;
  }
  // Generic container-like unknowns (activities without connections) drill
  // into their own hydrated children.
  if ("activities" in holder && Array.isArray(holder.activities)) return activity;
  return null;
}

/** Whether a chain layout applies to the drilled-into container. */
export function isChainContainer(activity: Activity): boolean {
  if (activity instanceof Flowchart) return false;
  if (!("activities" in activity)) return false;
  const connections = (activity as { connections?: unknown }).connections;
  // Unmirrored order-executing containers (Sequence 等) carry no connections;
  // hydrated GenericActivity defaults connections to [], which still means chain.
  return !Array.isArray(connections) || connections.length === 0;
}
