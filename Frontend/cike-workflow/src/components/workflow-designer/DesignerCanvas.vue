<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from "vue"
import { Graph } from "@antv/x6"
import type { CanvasProjection } from "@/core/designer/projection"
import { getCanvasState, type DesignerCanvasMeta } from "@/core/designer/metadata"
import { CIKE_NODE_SHAPE, registerDesignerShapes } from "./nodes/register"

const props = defineProps<{
  projection: CanvasProjection
  interactive: boolean
  selectedId: string | null
  /** Identity of the current drill level; changing it restores its viewport. */
  entryKey: string
  entryActivity: unknown
}>()

const emit = defineEmits<{
  nodeClick: [activityId: string]
  nodeDblclick: [activityId: string]
  nodeMoved: [payload: { id: string; x: number; y: number; from: { x: number; y: number } | null }]
  viewportChanged: [state: DesignerCanvasMeta]
}>()

const containerRef = ref<HTMLDivElement>()
let graph: Graph | null = null
/** Positions captured on node:mousedown for move-command undo. */
const dragOrigins = new Map<string, { x: number; y: number } | null>()

onMounted(() => {
  registerDesignerShapes()
  graph = new Graph({
    container: containerRef.value!,
    grid: false,
    interacting: props.interactive,
    mousewheel: { enabled: true, factor: 1.2, zoomAtMousePosition: true },
    panning: { enabled: true, eventTypes: ["leftMouseDown", "rightMouseDown"] },
  })
  graph.on("node:click", ({ node }) => emit("nodeClick", String(node.id)))
  graph.on("blank:click", () => emit("nodeClick", ""))
  graph.on("node:dblclick", ({ node }) => emit("nodeDblclick", String(node.id)))
  graph.on("node:mousedown", ({ node }) => {
    const activity = props.entryActivity as { activities?: { id: string; metadata?: { designer?: { x?: number; y?: number } } }[] } | undefined
    const child = activity && Array.isArray(activity.activities)
      ? activity.activities.find((a) => a.id === node.id)
      : undefined
    const designer = child?.metadata?.designer
    const from = designer && typeof designer.x === "number" && typeof designer.y === "number"
      ? { x: designer.x, y: designer.y }
      : null
    dragOrigins.set(String(node.id), from)
  })
  graph.on("node:moved", ({ node }) => {
    const { x, y } = node.getPosition()
    emit("nodeMoved", { id: String(node.id), x, y, from: dragOrigins.get(String(node.id)) ?? null })
  })
  graph.on("scale", ({ sx }) => {
    const translation = graph!.translate()
    emit("viewportChanged", { zoom: sx, panX: translation.tx, panY: translation.ty })
  })
  graph.on("translate", ({ tx, ty }) => {
    emit("viewportChanged", { zoom: graph!.zoom(), panX: tx, panY: ty })
  })
  renderProjection()
  applyViewport()
})

watch(() => props.projection, renderProjection)
watch(() => props.entryKey, () => applyViewport())

onBeforeUnmount(() => {
  graph?.dispose()
  graph = null
})

function applyViewport(): void {
  if (!graph) return
  const saved = getCanvasState(props.entryActivity as never)
  if (saved) {
    graph.scale(saved.zoom)
    graph.translate(saved.panX, saved.panY)
  }
}

function renderProjection(): void {
  if (!graph) return
  const cells: Record<string, unknown>[] = []
  for (const node of props.projection.nodes) {
    cells.push({
      shape: CIKE_NODE_SHAPE,
      id: node.id,
      x: node.x,
      y: node.y,
      data: node.data,
      ports: {
        items: node.data.ports.map((port) => ({
          id: port,
          group: "out",
          attrs: { label: { text: port, x: 9, y: 3 } },
        })),
      },
    })
  }
  for (const edge of props.projection.edges) {
    cells.push({
      shape: "edge",
      id: edge.id,
      zIndex: 0,
      source: edge.sourcePort ? { cell: edge.source, port: edge.sourcePort } : { cell: edge.source },
      target: { cell: edge.target },
      attrs: {
        line: {
          stroke: "currentColor",
          strokeWidth: 1.5,
          ...(edge.visual ? { strokeDasharray: "4 4" } : {}),
        },
      },
    })
  }
  graph.fromJSON({ cells })
}
</script>

<template>
  <div ref="containerRef" class="canvas-surface h-full w-full">
    <!-- Theme-aware edge/port styling: SVG presentation attributes cannot use
         CSS variables, so colors are applied through currentColor + CSS. -->
    <style scoped>
      .canvas-surface {
        background-image: radial-gradient(circle, var(--border) 1px, transparent 1px);
        background-size: 20px 20px;
        color: var(--border);
      }
      .canvas-surface :deep(.x6-port-body circle) {
        stroke: var(--border);
        fill: var(--background);
      }
      .canvas-surface :deep(.x6-port-body text) {
        fill: var(--muted-foreground);
        font-size: 10px;
        user-select: none;
      }
    </style>
  </div>
</template>
