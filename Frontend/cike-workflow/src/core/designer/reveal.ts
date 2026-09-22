import { Activity } from "../abstracts/Activity";
import { ensureDrillTarget } from "./drill";

/**
 * Pure resolution of a validation problem's location into the drill steps needed
 * to reveal it on the canvas. Kept free of designer state so it is unit-testable
 * at the core seam; the composable applies the result via its drill infrastructure.
 *
 * A NodeId chain (ADR 0003) is the backend activity path joined by ":", e.g.
 * "fc-root:a-for:body-fc:inner-start". Drilling into a loop lands directly on its
 * `body`, so the body segment is consumed by a single drill — the walk skips it.
 */
export interface RevealPath {
  /** Activities to drill into, in order; each is a node on the current canvas. */
  drillTargets: Activity[];
  /** Activity id to select on the final canvas (may be the container itself). */
  targetId: string;
}

type ChildHolder = { activities?: unknown; body?: unknown; root?: unknown };

/** Direct children of a container across every sub-shape (activities/body/root). */
function childActivities(container: Activity): Activity[] {
  const holder = container as unknown as ChildHolder;
  const out: Activity[] = [];
  if (Array.isArray(holder.activities)) {
    for (const child of holder.activities) if (child instanceof Activity) out.push(child);
  }
  if (holder.body instanceof Activity) out.push(holder.body);
  if (holder.root instanceof Activity) out.push(holder.root);
  return out;
}

/** Finds a direct child of `container` by activity id, across all sub-shapes. */
export function findChildById(container: Activity, id: string): Activity | null {
  for (const child of childActivities(container)) if (child.id === id) return child;
  return null;
}

/** Depth-first path of activities from `container` down to the one with `id`. */
function findPathById(container: Activity, id: string, trail: Activity[] = []): Activity[] | null {
  const next = [...trail, container];
  if (container.id === id) return next;
  for (const child of childActivities(container)) {
    const found = findPathById(child, id, next);
    if (found) return found;
  }
  return null;
}

/** Walks an activity-id chain from the root, collecting drill steps + target. */
function walkChain(root: Activity, chain: string[]): RevealPath | null {
  if (chain[0] !== root.id) return null;
  const drillTargets: Activity[] = [];
  let current: Activity = root;
  let i = 1;
  while (i < chain.length - 1) {
    const child = findChildById(current, chain[i]!);
    if (!child) return null;
    const drilled = ensureDrillTarget(child);
    if (!drilled) return null;
    drillTargets.push(child);
    current = drilled;
    // A loop/composite drills into its body/root — that is the next chain
    // segment, so consume it. A flowchart drills into itself.
    i = current.id === chain[i + 1] ? i + 2 : i + 1;
  }
  const targetId = chain[chain.length - 1]!;
  // The target must be a node on the final canvas, or the canvas container itself
  // (a root-level problem navigates to the level with no node to highlight).
  if (targetId !== current.id && !findChildById(current, targetId)) return null;
  return { drillTargets, targetId };
}

/**
 * Resolves how to reveal a validation problem: prefer its NodeId chain (direct,
 * no tree search); fall back to an activityId depth-first search when NodeId is
 * absent. Returns null when the problem cannot be located (workflow-level, or a
 * stale reference to a since-deleted node).
 */
export function resolveRevealPath(
  root: Activity,
  nodeId?: string | null,
  activityId?: string | null,
): RevealPath | null {
  if (nodeId) {
    const chain = nodeId.split(":").filter((segment) => segment.length > 0);
    if (chain.length > 0) return walkChain(root, chain);
  }
  if (activityId) {
    const path = findPathById(root, activityId);
    if (path) return walkChain(root, path.map((activity) => activity.id));
  }
  return null;
}
