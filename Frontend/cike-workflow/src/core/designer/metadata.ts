import type { IActivity } from "../abstracts/Activity";

/**
 * Designer persistence lives in the activity's free-form `metadata` dictionary
 * under the reserved `designer` key (see repo CONTEXT.md / spec issue #2).
 * Node positions are stored per activity; canvas viewport is stored on the
 * container activity that owns the level.
 */

export interface DesignerNodeMeta {
  x: number;
  y: number;
}

export interface DesignerNodeSize {
  width: number;
  height: number;
}

export interface DesignerCanvasMeta {
  zoom: number;
  panX: number;
  panY: number;
}

export interface DesignerMeta extends Partial<DesignerNodeMeta>, Partial<DesignerNodeSize> {
  canvas?: DesignerCanvasMeta;
}

export function getDesignerMeta(activity: IActivity): DesignerMeta {
  const value = activity.metadata?.["designer"];
  return value && typeof value === "object" ? (value as DesignerMeta) : {};
}

/** Mutation entry point; commands (ticket #5) wrap calls to this. */
export function patchDesignerMeta(activity: IActivity, patch: Partial<DesignerMeta>): void {
  const metadata = activity.metadata ?? (activity.metadata = {});
  const current = getDesignerMeta(activity);
  metadata["designer"] = { ...current, ...patch };
}

export function getNodePosition(activity: IActivity): DesignerNodeMeta | null {
  const meta = getDesignerMeta(activity);
  return typeof meta.x === "number" && typeof meta.y === "number" ? { x: meta.x, y: meta.y } : null;
}

export function setNodePosition(activity: IActivity, position: DesignerNodeMeta): void {
  patchDesignerMeta(activity, { x: position.x, y: position.y });
}

/** Drops the node position while preserving other designer metadata (e.g. canvas viewport). */
export function clearNodePosition(activity: IActivity): void {
  const meta = activity.metadata?.["designer"];
  if (!meta || typeof meta !== "object") return;
  const rest: DesignerMeta = { ...(meta as DesignerMeta) };
  delete rest.x;
  delete rest.y;
  if (Object.keys(rest).length === 0) delete activity.metadata!["designer"];
  else activity.metadata!["designer"] = rest;
}

export function getCanvasState(activity: IActivity): DesignerCanvasMeta | null {
  const canvas = getDesignerMeta(activity).canvas;
  return canvas && typeof canvas.zoom === "number" ? canvas : null;
}

export function setCanvasState(activity: IActivity, state: DesignerCanvasMeta): void {
  patchDesignerMeta(activity, { canvas: state });
}

export function getNodeSize(activity: IActivity): DesignerNodeSize | null {
  const meta = getDesignerMeta(activity);
  return typeof meta.width === "number" && typeof meta.height === "number"
    ? { width: meta.width, height: meta.height }
    : null;
}

export function setNodeSize(activity: IActivity, size: DesignerNodeSize): void {
  patchDesignerMeta(activity, { width: size.width, height: size.height });
}

/** Drops the node size while preserving other designer metadata. */
export function clearNodeSize(activity: IActivity): void {
  const meta = activity.metadata?.["designer"];
  if (!meta || typeof meta !== "object") return;
  const rest: DesignerMeta = { ...(meta as DesignerMeta) };
  delete rest.width;
  delete rest.height;
  if (Object.keys(rest).length === 0) delete activity.metadata!["designer"];
  else activity.metadata!["designer"] = rest;
}
