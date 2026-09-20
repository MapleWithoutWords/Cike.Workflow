import { computed, ref, shallowRef } from "vue"
import {
  getApiV1CommonsActivityDescriptors,
  getApiV1WorkflowDefinitionsById,
  postApiV1WorkflowDefinitionsSaveById,
} from "@/api/generated"
import type { Activity, IActivity } from "@/core/abstracts/Activity"
import { Activity as ActivityClass } from "@/core/abstracts/Activity"
import { Flowchart } from "@/core/activities/Flowchart"
import { ActivityConnection } from "@/core/models/ActivityConnection"
import { ActivityEndpoint } from "@/core/models/ActivityEndpoint"
import {
  CommandStack,
  makeAddNodeCommand,
  makeConnectCommand,
  makeDisconnectCommand,
  makeMoveNodeCommand,
  makeRemoveNodeCommand,
  type DesignerCommand,
} from "@/core/designer/commands"
import { buildPaletteGroups, type PaletteGroup } from "@/core/designer/palette"
import type { InputDescriptor, VariableDefinition } from "@/api/generated"
import { ensureDrillTarget, isChainContainer } from "@/core/designer/drill"
import { projectOrderedChain, projectFlowchart, projectCanvas, canDrillInto, type CanvasProjection } from "@/core/designer/projection"
import { activityShortName, resolveActivityClass } from "@/core/designer/registry"
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
  /** Bumped on every model mutation (command/undo/redo) so model-derived
   *  computeds re-run — the domain model is held in shallowRefs and commands
   *  mutate plain nested arrays that Vue cannot track on its own. */
  const revision = ref(0)
  const paletteGroups = shallowRef<PaletteGroup[]>([])
  /** Wire type → descriptor lookup (inputs drive the generic property form). */
  const descriptorByType = shallowRef<Map<string, { inputs?: InputDescriptor[] }>>(new Map())
  /** Options-level workflow variables. */
  const variables = shallowRef<VariableDefinition[]>([])

  const currentEntry = computed<DrillEntry | null>(() => drillStack.value[drillStack.value.length - 1] ?? null)

  const breadcrumb = computed(() => drillStack.value.map((entry) => entry.title))

  const projection = computed<CanvasProjection>(() => {
    void revision.value
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
    void revision.value
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

  /** Inbound edge count per activity id on the current level (MergeMode UI gate). */
  const connectionTargets = computed(() => {
    const map = new Map<string, number>()
    for (const edge of projection.value.edges) {
      map.set(edge.target, (map.get(edge.target) ?? 0) + 1)
    }
    return map
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
      variables.value = ((data.options as { variables?: VariableDefinition[] } | null)?.variables ?? []) as VariableDefinition[]
      definitionName.value = data.name || data.definitionId || "未命名"
      root.value = fromWireActivity((data.root ?? {}) as WireActivity)
      drillStack.value = [{ activity: root.value, title: definitionName.value }]
      selectedActivityId.value = null
      commandStack.clear()
      refreshUndoFlags()
      void loadPalette()
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
    revision.value++
    refreshUndoFlags()
  }

  function undo(): void {
    commandStack.undo()
    revision.value++
    refreshUndoFlags()
  }

  function redo(): void {
    commandStack.redo()
    revision.value++
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

  async function loadPalette(): Promise<void> {
    try {
      const { data } = await getApiV1CommonsActivityDescriptors({})
      const descriptors = (data ?? []) as Array<{ typeName?: string; inputs?: InputDescriptor[] }>
      paletteGroups.value = buildPaletteGroups(descriptors)
      const lookup = new Map<string, { inputs?: InputDescriptor[] }>()
      for (const descriptor of descriptors) {
        if (descriptor.typeName) lookup.set(descriptor.typeName, { inputs: descriptor.inputs })
      }
      descriptorByType.value = lookup
    } catch {
      paletteGroups.value = []
    }
  }

  /** Adds a node of the given wire type; point is canvas-local, auto-staggered
   *  down-right while it would overlap an existing node. */
  function addNode(typeName: string, point: { x: number; y: number }): void {
    const Ctor = resolveActivityClass(typeName)
    if (!Ctor) return
    const entry = currentEntry.value
    if (!entry || entry.chainChildren) return
    const activity = new Ctor()
    const occupied = new Set(projection.value.nodes.map((node) => `${Math.round(node.x)},${Math.round(node.y)}`))
    let position = { x: Math.round(point.x), y: Math.round(point.y) }
    while (occupied.has(`${position.x},${position.y}`)) {
      position = { x: position.x + 30, y: position.y + 70 }
    }
    const container = entry.activity as unknown as { activities: IActivity[] }
    executeCommand(makeAddNodeCommand(container as never, activity, position))
    selectedActivityId.value = activity.id
  }

  /** Replaces the options-level variables through the command stack so the
   *  edit participates in undo/redo like every other designer edit. */
  function setVariables(list: VariableDefinition[]): void {
    const from = savedOptions.value["variables"] as VariableDefinition[] | undefined
    executeCommand({
      label: "编辑工作流变量",
      apply: () => {
        savedOptions.value = { ...savedOptions.value, variables: list }
        variables.value = list
      },
      undo: () => {
        const restored = { ...savedOptions.value }
        if (from == null) delete restored["variables"]
        else restored["variables"] = from
        savedOptions.value = restored
        variables.value = from ?? []
      },
      redo: () => {
        savedOptions.value = { ...savedOptions.value, variables: list }
        variables.value = list
      },
    })
  }

  const selectedEdgeId = ref<string | null>(null)

  /** Creates a connection; returns false (rejecting the canvas edge) on duplicates. */
  function connect(payload: { source: string; sourcePort?: string; target: string }): boolean {
    const entry = currentEntry.value
    if (!entry || entry.chainChildren) return false
    if (payload.source === payload.target) return false
    const container = entry.activity as unknown as { connections?: ActivityConnection[] }
    const connections = container.connections ?? []
    const duplicate = connections.some(
      (connection) =>
        connection.source.activityId === payload.source &&
        connection.source.port === payload.sourcePort &&
        connection.target.activityId === payload.target,
    )
    if (duplicate) return false
    executeCommand(
      makeConnectCommand(
        container as never,
        new ActivityConnection(new ActivityEndpoint(payload.source, payload.sourcePort), new ActivityEndpoint(payload.target)),
      ),
    )
    return true
  }

  function removeEdge(edgeId: string): void {
    const entry = currentEntry.value
    if (!entry) return
    const edge = projection.value.edges.find((candidate) => candidate.id === edgeId)
    if (!edge || edge.visual) return
    const container = entry.activity as unknown as { connections?: ActivityConnection[] }
    const connections = container.connections ?? []
    const connection = connections.find(
      (candidate) =>
        candidate.source.activityId === edge.source &&
        (candidate.source.port ?? undefined) === (edge.sourcePort ?? undefined) &&
        candidate.target.activityId === edge.target,
    )
    if (!connection) return
    executeCommand(makeDisconnectCommand(container as never, connection))
    if (selectedEdgeId.value === edgeId) selectedEdgeId.value = null
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
    paletteGroups,
    descriptorByType,
    connectionTargets,
    variables,
    setVariables,
    addNode,
    selectedEdgeId,
    connect,
    removeEdge,
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
