import type { Activity } from "../abstracts/Activity";
import { Break } from "../activities/Break";
import { End } from "../activities/End";
import { Fault } from "../activities/Fault";
import { Flowchart } from "../activities/Flowchart";
import { For } from "../activities/For";
import { ForEach } from "../activities/ForEach";
import { If } from "../activities/If";
import { RunJavaScript } from "../activities/RunJavaScript";
import { SendHttpRequest } from "../activities/SendHttpRequest";
import { Start } from "../activities/Start";
import { Switch } from "../activities/Switch";
import { While } from "../activities/While";

/**
 * Registry of frontend-mirrored activity classes, keyed by the short type name
 * (the segment after the last dot of the wire `type`, e.g. "Cike.If" -> "If").
 * Unknown types are not registered here; they fall back to GenericActivity.
 */
export const ACTIVITY_REGISTRY: Record<string, new () => Activity> = {
  Start,
  End,
  Fault,
  Flowchart,
  For,
  ForEach,
  If,
  RunJavaScript,
  SendHttpRequest,
  Switch,
  While,
  Break,
};

export function activityShortName(type: string): string {
  const index = type.lastIndexOf(".");
  return index >= 0 ? type.slice(index + 1) : type;
}

export function resolveActivityClass(type: string): (new () => Activity) | null {
  return ACTIVITY_REGISTRY[activityShortName(type)] ?? null;
}
