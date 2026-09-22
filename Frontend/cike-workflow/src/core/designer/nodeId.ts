/**
 * NodeId computation, kept byte-for-byte consistent with the backend
 * `ActivityNode.NodeId` scheme (ADR 0003): an activity's NodeId is its
 * ancestors' activity ids joined by ":", then its own id. The root has no
 * ancestors, so its NodeId is simply its own id.
 *
 * NodeId is a structural path identity derived purely from containment
 * (container `activities` / composite `root` / loop `body`). Connections never
 * enter the port graph, so connect/disconnect/move/edit do not change a NodeId —
 * only adding a node (O(1), from its parent's NodeId) or reparenting would.
 */
export function computeNodeId(
  parentNodeId: string | null | undefined,
  ownId: string,
): string {
  return parentNodeId ? `${parentNodeId}:${ownId}` : ownId;
}
