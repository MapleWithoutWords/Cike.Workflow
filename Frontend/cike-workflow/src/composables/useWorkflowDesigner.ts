import { computed, ref, shallowRef } from "vue"
import {
  getApiV1WorkflowDefinitionsById,
  postApiV1WorkflowDefinitionsSaveById,
} from "@/api/generated"
import type { Activity, IActivity } from "@/core/abstracts/Activity"
import { Activity as ActivityClass } from "@/core/abstracts/Activity"
import { Flowchart } from "@/core/activities/Flowchart"
import {
  CommandStack,
  makeMoveNodeCommand,
  makeRemoveNodeCommand,
  type DesignerCommand,
} from "@/core/designer/commands"
import { ensureDrillTarget, isChainContainer } from "@/core/designer/drill"
import { projectOrderedChain, projectFlowchart, projectCanvas, canDrillInto, type CanvasProjection } from "@/core/designer/projection"
import { activityShortName } from "@/core/designer/registry"
import { fromWireActivity, toWireActivity, type WireActivity } from "@/core/designer/serialization"
import { getCanvasState, setCanvasState, type DesignerCanvasMeta } from "@/core/designer/metadata"

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
  const rowId = ref<string | null>(null)
  const savedOptions = shallowRef<Record<string, unknown>>({})
  const saving = ref(false)
  const saveError = ref<string | null>(null)
  const lastSavedAt = ref<Date | null>(null)
  const canUndo = ref(false)
  const canRedo = ref(false)
  const commandStack = new CommandStack()

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

  /** Current level misses a Start node (backend publish validation gate). */
  const missingStartNode = computed(() => {
    const children = currentChildren.value
    if (children.length === 0) return false
    return !children.some((child) => activityShortName(child.type) === "Start")
  })

  async function load(definitionRowId: string): Promise<void> {
    loading.value = true
    loadError.value = null
    try {
      const { data, error } = await getApiV1WorkflowDefinitionsById({ path: { id: definitionRowId } })
      if (error || !data) {
        loadError.value = error ? String(error) : "加载定义失败"
        return
      }
      rowId.value = definitionRowId
      savedOptions.value = (data.options ?? {}) as Record<string, unknown>
      definitionName.value = data.name || data.definitionId || "未命名"
      root.value = fromWireActivity((data.root ?? {}) as WireActivity)
      drillStack.value = [{ activity: root.value, title: definitionName.value }]
      selectedActivityId.value = null
      commandStack.clear()
      refreshUndoFlags()
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

  function refreshUndoFlags(): void {
    canUndo.value = commandStack.canUndo()
    canRedo.value = commandStack.canRedo()
  }

  function executeCommand(command: DesignerCommand): void {
    commandStack.execute(command)
    refreshUndoFlags()
  }

  function undo(): void {
    commandStack.undo()
    refreshUndoFlags()
  }

  function redo(): void {
    commandStack.redo()
    refreshUndoFlags()
  }

  function moveNode(payload: { id: string; x: number; y: number; from: { x: number; y: number } | null }): void {
    const activity = currentChildren.value.find((child) => child.id === payload.id)
    if (!activity) return
    executeCommand(makeMoveNodeCommand(activity as IActivity, payload.from, { x: payload.x, y: payload.y }))
  }

  /** Builds the delete command; the UI confirms before executing it. */
  function buildRemoveCommand(activityId: string): DesignerCommand | null {
    const entry = currentEntry.value
    if (!entry) return null
    const container = entry.activity as unknown as { activities: IActivity[]; connections?: unknown[] }
    return makeRemoveNodeCommand(container as never, activityId)
  }

  function removeNode(activityId: string): void {
    const command = buildRemoveCommand(activityId)
    if (!command) return
    executeCommand(command)
    if (selectedActivityId.value === activityId) selectedActivityId.value = null
  }

  function saveViewport(state: DesignerCanvasMeta): void {
    const entry = currentEntry.value
    if (!entry) return
    setCanvasState(entry.activity, state)
  }

  function getViewport(): DesignerCanvasMeta | null {
    const entry = currentEntry.value
    if (!entry) return null
    return getCanvasState(entry.activity)
  }

  async function save(): Promise<void> {
    if (!root.value || rowId.value == null || saving.value) return
    saving.value = true
    saveError.value = null
    try {
      const { error } = await postApiV1WorkflowDefinitionsSaveById({
        path: { id: rowId.value },
        body: {
          root: toWireActivity(root.value) as never,
          options: savedOptions.value as never,
        },
      })
      if (error) {
        saveError.value = typeof error === "string" ? error : JSON.stringify(error)
        return
      }
      lastSavedAt.value = new Date()
    } catch (err) {
      saveError.value = String(err)
    } finally {
      saving.value = false
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
    currentChildren,
    breadcrumb,
    projection,
    missingStartNode,
    saving,
    saveError,
    lastSavedAt,
    canUndo,
    canRedo,
    load,
    drillInto,
    popTo,
    executeCommand,
    buildRemoveCommand,
    removeNode,
    undo,
    redo,
    moveNode,
    saveViewport,
    getViewport,
    save,
  }
}

export type WorkflowDesignerState = ReturnType<typeof useWorkflowDesigner>
