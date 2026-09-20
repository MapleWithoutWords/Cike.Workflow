import { register } from "@antv/x6-vue-shape"
import DesignerNode from "./DesignerNode.vue"

export const CIKE_NODE_SHAPE = "cike-node"
export const NODE_WIDTH = 180
export const NODE_HEIGHT = 44

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
