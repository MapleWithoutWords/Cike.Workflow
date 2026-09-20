import type { IActivity } from "../abstracts/Activity";
import type { ActivityConnection } from "../models/ActivityConnection";

export interface Point {
  x: number;
  y: number;
}

const ORIGIN_X = 80;
const ORIGIN_Y = 80;
const COLUMN_WIDTH = 280;
const ROW_HEIGHT = 140;

/**
 * Deterministic fallback layout for flowcharts without saved positions.
 * Nodes are layered by BFS depth from start nodes (no inbound connections);
 * layers become columns, visits within a layer become rows.
 */
export function computeAutoLayout(
  activities: IActivity[],
  connections: ActivityConnection[],
): Map<string, Point> {
  const positions = new Map<string, Point>();
  if (activities.length === 0) return positions;

  const inbound = new Map<string, number>();
  const outbound = new Map<string, string[]>();
  for (const activity of activities) {
    inbound.set(activity.id, 0);
    outbound.set(activity.id, []);
  }
  for (const connection of connections) {
    if (!inbound.has(connection.target.activityId)) continue;
    inbound.set(connection.target.activityId, (inbound.get(connection.target.activityId) ?? 0) + 1);
    outbound.get(connection.source.activityId)?.push(connection.target.activityId);
  }

  const depth = new Map<string, number>();
  const queue: string[] = [];
  for (const activity of activities) {
    if ((inbound.get(activity.id) ?? 0) === 0) {
      depth.set(activity.id, 0);
      queue.push(activity.id);
    }
  }
  // Cycle-only graphs have no start; seed with the first activity.
  if (queue.length === 0 && activities.length > 0) {
    depth.set(activities[0].id, 0);
    queue.push(activities[0].id);
  }

  const order: string[] = [];
  const maxDepthByRow = new Map<number, number>();
  while (queue.length > 0) {
    const id = queue.shift()!;
    const currentDepth = depth.get(id) ?? 0;
    if (!maxDepthByRow.has(currentDepth)) maxDepthByRow.set(currentDepth, 0);
    order.push(id);
    for (const next of outbound.get(id) ?? []) {
      if (depth.has(next)) continue;
      depth.set(next, currentDepth + 1);
      queue.push(next);
    }
  }

  // Unreached nodes (disconnected or cycle-only) go to the last column + 1.
  const maxDepth = order.reduce((acc, id) => Math.max(acc, depth.get(id) ?? 0), 0);
  for (const activity of activities) {
    if (!depth.has(activity.id)) depth.set(activity.id, maxDepth + 1);
  }

  const rowIndexByColumn = new Map<number, number>();
  for (const id of order) {
    const column = depth.get(id) ?? 0;
    const row = rowIndexByColumn.get(column) ?? 0;
    rowIndexByColumn.set(column, row + 1);
    positions.set(id, { x: ORIGIN_X + column * COLUMN_WIDTH, y: ORIGIN_Y + row * ROW_HEIGHT });
  }
  // Late-assigned nodes (disconnected/cycle-only) that were not in `order`.
  let overflowRow = rowIndexByColumn.get(maxDepth + 1) ?? 0;
  for (const activity of activities) {
    if (positions.has(activity.id)) continue;
    positions.set(activity.id, {
      x: ORIGIN_X + (maxDepth + 1) * COLUMN_WIDTH,
      y: ORIGIN_Y + overflowRow * ROW_HEIGHT,
    });
    overflowRow += 1;
  }

  return positions;
}
