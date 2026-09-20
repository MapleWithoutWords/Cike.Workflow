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

export interface DesignerCanvasMeta {
  zoom: number;
  panX: number;
  panY: number;
}

export interface DesignerMeta extends Partial<DesignerNodeMeta> {
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

export function getCanvasState(activity: IActivity): DesignerCanvasMeta | null {
  const canvas = getDesignerMeta(activity).canvas;
  return canvas && typeof canvas.zoom === "number" ? canvas : null;
}

export function setCanvasState(activity: IActivity, state: DesignerCanvasMeta): void {
  patchDesignerMeta(activity, { canvas: state });
}
