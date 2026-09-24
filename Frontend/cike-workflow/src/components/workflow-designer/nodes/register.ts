import { register, getTeleport } from "@antv/x6-vue-shape"
import DesignerNode from "./DesignerNode.vue"

export const CIKE_NODE_SHAPE = "cike-node"
export const NODE_WIDTH = 180
export const NODE_HEIGHT = 44
/** Vertical space each port occupies on the node edge. */
export const PORT_SPACING = 22
/** Top+bottom padding inside the port area. */
export const NODE_PADDING = 12

/**
 * x6-vue-shape v3 renders Vue node content through a teleport container that
 * must be mounted in the Vue tree; calling getTeleport() also flips the shared
 * `active` flag on, without which node components never render. Created once at
 * module scope so the flag is enabled before any graph builds its cells.
 */
export const TeleportContainer = getTeleport()

let registered = false

/** Idempotent shape registration; safe to call before every Graph creation. */
export function registerDesignerShapes(): void {
  if (registered) return
  registered = true
  register({
    shape: CIKE_NODE_SHAPE,
    component: DesignerNode,
    width: NODE_WIDTH,
    height: NODE_HEIGHT,
    ports: {
      groups: {
        in: {
          position: { name: "left" },
          markup: [{ tagName: "circle", selector: "dot" }],
          attrs: {
            // 'passive': can be a connection target only, never initiates a drag.
            dot: { magnet: "passive", r: 5 },
          },
        },
        out: {
          position: { name: "right" },
          markup: [
            { tagName: "circle", selector: "dot" },
            { tagName: "text", selector: "label" },
          ],
          attrs: {
            dot: { magnet: true, r: 5 },
          },
        },
      },
    },
  })
}
