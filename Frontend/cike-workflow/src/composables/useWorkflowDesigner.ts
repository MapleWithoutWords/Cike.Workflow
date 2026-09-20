import { computed, ref, shallowRef } from "vue"
import { getApiV1WorkflowDefinitionsById } from "@/api/generated"
import type { Activity, IActivity } from "@/core/abstracts/Activity"
import { Activity as ActivityClass } from "@/core/abstracts/Activity"
import { Flowchart } from "@/core/activities/Flowchart"
import { ensureDrillTarget, isChainContainer } from "@/core/designer/drill"
import { projectOrderedChain, projectFlowchart, projectCanvas, canDrillInto, type CanvasProjection } from "@/core/designer/projection"
import { activityShortName } from "@/core/designer/registry"
import { fromWireActivity, type WireActivity } from "@/core/designer/serialization"

/** One drill-down level: the container owning the canvas content. */
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
    if ("activities" in entry.activity && Array.isArray((entry.activity as { activities?: unknown }).activities)) {
      return projectCanvas(
        entry.activity as unknown as { activities: IActivity[]; connections: import("@/core/models/ActivityConnection").ActivityConnection[] },
      )
    }
    return { nodes: [], edges: [] }
  })

  const currentChildren = computed<IActivity[]>(() => {
    const entry = currentEntry.value
    if (!entry) return []
    if (entry.chainChildren) return entry.chainChildren
    if (entry.activity instanceof Flowchart) return entry.activity.activities
    const activities = (entry.activity as { activities?: unknown }).activities
    return Array.isArray(activities) ? (activities as IActivity[]) : []
  })

  const selectedActivity = computed<IActivity | null>(() => {
    if (!selectedActivityId.value) return null
    return currentChildren.value.find((child) => child.id === selectedActivityId.value) ?? null
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

  function drillInto(activity: IActivity): void {
    if (!canDrillInto(activity)) return
    if (!(activity instanceof ActivityClass)) return
    const target = ensureDrillTarget(activity)
    if (!target) return
    const title = activity.name ?? activityShortName(activity.type)
    const entry: DrillEntry = isChainContainer(target)
      ? { activity: target, title, chainChildren: (target as unknown as { activities: IActivity[] }).activities }
      : { activity: target, title }
    drillStack.value = [...drillStack.value, entry]
    selectedActivityId.value = null
  }

  function popTo(index: number): void {
    if (index < 0 || index >= drillStack.value.length) return
    drillStack.value = drillStack.value.slice(0, index + 1)
    selectedActivityId.value = null
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
    currentChildren,
    breadcrumb,
    projection,
    load,
    drillInto,
    popTo,
  }
}

export type WorkflowDesignerState = ReturnType<typeof useWorkflowDesigner>
