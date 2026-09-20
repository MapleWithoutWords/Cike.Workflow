import { computed, ref, shallowRef } from "vue"
import { getApiV1WorkflowDefinitionsById } from "@/api/generated"
import type { Activity, IActivity } from "@/core/abstracts/Activity"
import { Flowchart } from "@/core/activities/Flowchart"
import { projectOrderedChain, projectFlowchart, type CanvasProjection } from "@/core/designer/projection"
import { fromWireActivity, type WireActivity } from "@/core/designer/serialization"

/** One drill-down level: the flowchart (or chain container) owning the canvas. */
export interface DrillEntry {
  /** Container whose children are displayed on this level. */
  activity: Activity
  title: string
  /** When set, this level is an ordered chain (no connections in the model). */
  chainChildren?: IActivity[]
}

export function useWorkflowDesigner() {
  const root = shallowRef<Activity | null>(null)
  const definitionName = ref("")
  const loadError = ref<string | null>(null)
  const loading = ref(false)
  const drillStack = shallowRef<DrillEntry[]>([])
  const selectedActivityId = ref<string | null>(null)

  const currentEntry = computed<DrillEntry | null>(() => drillStack.value[drillStack.value.length - 1] ?? null)

  const breadcrumb = computed(() => drillStack.value.map((entry) => entry.title))

  const projection = computed<CanvasProjection>(() => {
    const entry = currentEntry.value
    if (!entry) return { nodes: [], edges: [] }
    if (entry.chainChildren) return projectOrderedChain(entry.chainChildren)
    if (entry.activity instanceof Flowchart) return projectFlowchart(entry.activity)
    return { nodes: [], edges: [] }
  })

  const selectedActivity = computed<IActivity | null>(() => {
    const entry = currentEntry.value
    if (!entry || !selectedActivityId.value) return null
    const children = entry.chainChildren ?? (entry.activity instanceof Flowchart ? entry.activity.activities : [])
    return children.find((child) => child.id === selectedActivityId.value) ?? null
  })

  async function load(rowId: string): Promise<void> {
    loading.value = true
    loadError.value = null
    try {
      const { data, error } = await getApiV1WorkflowDefinitionsById({ path: { id: rowId } })
      if (error || !data) {
        loadError.value = error ? String(error) : "加载定义失败"
        return
      }
      definitionName.value = data.name || data.definitionId || "未命名"
      root.value = fromWireActivity((data.root ?? {}) as WireActivity)
      drillStack.value = [{ activity: root.value, title: definitionName.value }]
      selectedActivityId.value = null
    } finally {
      loading.value = false
    }
  }

  return {
    root,
    definitionName,
    loadError,
    loading,
    drillStack,
    selectedActivityId,
    selectedActivity,
    currentEntry,
    breadcrumb,
    projection,
    load,
  }
}

export type WorkflowDesignerState = ReturnType<typeof useWorkflowDesigner>
