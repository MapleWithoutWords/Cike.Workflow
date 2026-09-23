<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue"
import { useRoute, useRouter } from "vue-router"
import { ArrowLeft, History, Pencil, Redo2, RotateCcw, Trash2, Undo2, Upload } from "@lucide/vue"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import type { WorkflowDefinitionFolderItemDto } from "@/api/generated"
import DefinitionFormDialog from "@/components/DefinitionFormDialog.vue"
import ThemeToggle from "@/components/layout/ThemeToggle.vue"
import DesignerBreadcrumb from "./DesignerBreadcrumb.vue"
import DesignerCanvas from "./DesignerCanvas.vue"
import ActivityPalette from "./ActivityPalette.vue"
import RightToolDock from "./RightToolDock.vue"
import PublishDialog from "./PublishDialog.vue"
import ProblemListPanel from "./ProblemListPanel.vue"
import VersionHistorySheet from "./VersionHistorySheet.vue"

const props = defineProps<{ designer: WorkflowDesignerState }>()

const route = useRoute()
const router = useRouter()
const workspaceId = computed(() => route.params.workspaceId as string)

const canvasRef = ref<InstanceType<typeof DesignerCanvas> | null>(null)
const publishOpen = ref(false)
const historyOpen = ref(false)
const editOpen = ref(false)
/** Problem list dock open state; auto-expands when validation finds problems. */
const problemPanelOpen = ref(false)

// Keep the dock in sync with validation results without fighting the user:
// reveal newly-appearing problems (0→N) and auto-collapse once clean (→0), but
// leave N→M transitions alone so a manual collapse sticks while editing.
watch(
  () => props.designer.problems.value.length,
  (count, prev) => {
    if (prev === 0 && count > 0) problemPanelOpen.value = true
    else if (count === 0) problemPanelOpen.value = false
  },
)

/** Publish is gated by validation: check first, then either reveal problems or
 *  open the note dialog only when the canvas is clean (ADR 0002). */
async function onPublishClick(): Promise<void> {
  const found = await props.designer.validate()
  if (found.length > 0) {
    problemPanelOpen.value = true
    const first = found.find((problem) => problem.nodeId || problem.activityId)
    if (first) props.designer.revealActivity(first)
    return
  }
  if (props.designer.validationError.value) {
    // Could not verify the canvas — surface the transport error, do not publish.
    problemPanelOpen.value = true
    return
  }
  publishOpen.value = true
}

/** Synthetic shape for the reused metadata dialog (edit mode keyed by row id). */
const editDefinition = computed<WorkflowDefinitionFolderItemDto>(() => ({
  id: props.designer.rowId.value ?? undefined,
  data: {
    definitionId: props.designer.definitionId.value,
    name: props.designer.definitionName.value,
    description: props.designer.description.value,
    type: props.designer.definitionType.value,
    usableAsActivity: props.designer.usableAsActivity.value,
  } as WorkflowDefinitionFolderItemDto["data"],
}))

function goBack(): void {
  router.push({ name: "definitions", params: { workspaceId: workspaceId.value } })
}

function onMetadataSaved(): void {
  const rowId = props.designer.rowId.value
  if (rowId) props.designer.load(rowId)
}

const entryKey = computed(() => {
  const stack = props.designer.drillStack.value
  return stack.map((entry) => entry.activity.id).join("/")
})

// Delete is immediate — no confirm dialog. Edits are drafts and undo (Ctrl+Z)
// reverses a removal, so a confirmation step would only add friction.
function requestRemove(activityId: string): void {
  props.designer.removeNode(activityId)
}

function drillById(activityId: string): void {
  const activity = props.designer.currentChildren.value.find((child) => child.id === activityId)
  if (activity) props.designer.drillInto(activity)
}

function addAtCenter(typeName: string): void {
  props.designer.addNode(typeName, canvasRef.value?.viewportCenter() ?? { x: 80, y: 80 })
}

function onConnectRequest(payload: { edgeId: string; source: string; sourcePort?: string; target: string }): void {
  const accepted = props.designer.connect(payload)
  if (!accepted) canvasRef.value?.removeCellById(payload.edgeId)
}

function onKeydown(event: KeyboardEvent): void {
  // Read-only versions (historic) never accept keyboard mutations.
  if (props.designer.readonly.value) return
  if (event.key !== "Delete" && event.key !== "Backspace") return
  const target = event.target as HTMLElement | null
  if (target && (target.tagName === "INPUT" || target.tagName === "TEXTAREA" || target.isContentEditable)) return
  const edgeId = props.designer.selectedEdgeId.value
  if (edgeId) {
    event.preventDefault()
    props.designer.removeEdge(edgeId)
    return
  }
  const activityId = props.designer.selectedActivityId.value
  if (!activityId) return
  event.preventDefault()
  requestRemove(activityId)
}

onMounted(() => window.addEventListener("keydown", onKeydown))
onBeforeUnmount(() => window.removeEventListener("keydown", onKeydown))
</script>

<template>
  <div class="flex h-full min-h-0 flex-col">
    <header class="flex shrink-0 items-center justify-between border-b px-4 py-2">
      <div class="flex min-w-0 items-center gap-3">
        <Button variant="ghost" size="icon" title="返回" @click="goBack">
          <ArrowLeft :size="16" />
        </Button>
        <div class="flex min-w-0 items-center gap-2">
          <span class="truncate text-sm font-medium">{{ designer.definitionName.value }}</span>
          <Badge variant="secondary" class="font-mono">v{{ designer.version.value }}</Badge>
          <Badge v-if="designer.isPublished.value" class="bg-success/15 text-success border-transparent">已发布</Badge>
        </div>
        <DesignerBreadcrumb :entries="designer.breadcrumb.value" @select="(index: number) => designer.popTo(index)" />
        <span
          v-if="designer.readonly.value"
          class="flex shrink-0 items-center gap-2 rounded bg-info/15 px-2 py-0.5 text-xs text-info"
        >
          正在查看 v{{ designer.version.value }}（只读）
          <button type="button" class="inline-flex items-center gap-1 font-medium hover:underline" @click="designer.returnToLatest()">
            <RotateCcw :size="12" /> 返回最新
          </button>
        </span>
      </div>
      <div class="flex shrink-0 items-center gap-2">
        <Button variant="ghost" size="icon" :disabled="!designer.canUndo.value || designer.readonly.value" title="撤销" @click="designer.undo()">
          <Undo2 :size="16" />
        </Button>
        <Button variant="ghost" size="icon" :disabled="!designer.canRedo.value || designer.readonly.value" title="重做" @click="designer.redo()">
          <Redo2 :size="16" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          :disabled="!designer.selectedActivityId.value || designer.readonly.value"
          title="删除节点"
          @click="designer.selectedActivityId.value && requestRemove(designer.selectedActivityId.value)"
        >
          <Trash2 :size="16" />
        </Button>
        <div class="mx-1 h-5 w-px bg-border" />
        <ThemeToggle />
        <Button variant="ghost" size="icon" title="历史版本" @click="historyOpen = true">
          <History :size="16" />
        </Button>
        <Button variant="outline" size="sm" :disabled="designer.readonly.value" @click="editOpen = true">
          <Pencil :size="14" /> 编辑
        </Button>
        <Button size="sm" variant="ghost" :disabled="designer.saving.value || designer.readonly.value" @click="designer.save()">
          {{ designer.saving.value ? "保存中…" : "保存" }}
        </Button>
        <Button size="sm" :disabled="designer.readonly.value || designer.saving.value" @click="onPublishClick">
          <Upload :size="14" /> 发布
        </Button>
      </div>
    </header>
    <div class="flex min-h-0 flex-1">
      <ActivityPalette v-if="!designer.readonly.value" :groups="designer.paletteGroups.value" @add="(typeName: string) => addAtCenter(typeName)" />
      <div class="flex min-w-0 flex-1 flex-col">
        <div class="relative min-h-0 flex-1">
          <DesignerCanvas
            ref="canvasRef"
            :projection="designer.projection.value"
            :interactive="!designer.readonly.value"
            :selected-id="designer.selectedActivityId.value"
            :entry-key="entryKey"
            :entry-activity="designer.currentEntry.value?.activity ?? null"
            @node-click="(id: string) => { designer.selectedActivityId.value = id || null; designer.selectedEdgeId.value = null }"
            @node-dblclick="(id: string) => drillById(id)"
            @node-moved="(payload) => designer.moveNode(payload)"
            @viewport-changed="(state) => designer.saveViewport(state)"
            @drop-activity="(payload) => designer.addNode(payload.typeName, { x: payload.x, y: payload.y })"
            @edge-click="(edgeId: string) => { designer.selectedActivityId.value = null; designer.selectedEdgeId.value = edgeId }"
            @connect-request="onConnectRequest"
          />
          <div
            v-if="designer.loadError.value || designer.saveError.value"
            class="absolute inset-x-0 top-0 border-b bg-destructive/10 px-4 py-2 text-xs text-destructive"
          >
            {{ designer.loadError.value || designer.saveError.value }}
          </div>
          <div
            v-if="designer.lastSavedAt.value"
            class="absolute bottom-2 right-2 rounded bg-background/90 border px-2 py-1 text-xs text-muted-foreground"
          >
            已保存 {{ designer.lastSavedAt.value.toLocaleTimeString() }}
          </div>
        </div>
        <!-- 底部 dock 只占画布列（VS Code/IntelliJ 惯例）：侧栏保持全高，仅画布为它让高度。 -->
        <ProblemListPanel
          v-if="!designer.readonly.value"
          :problems="designer.problems.value"
          :validating="designer.validating.value"
          :validation-error="designer.validationError.value"
          :open="problemPanelOpen"
          @update:open="(open: boolean) => (problemPanelOpen = open)"
          @reveal="(problem) => designer.revealActivity(problem)"
        />
      </div>
      <RightToolDock :designer="designer" />
    </div>

    <PublishDialog :designer="designer" :open="publishOpen" @update:open="(open: boolean) => (publishOpen = open)" />

    <VersionHistorySheet :designer="designer" :open="historyOpen" @update:open="(open: boolean) => (historyOpen = open)" />

    <DefinitionFormDialog
      :open="editOpen"
      :definition="editDefinition"
      :folder-id="designer.folderId.value"
      :workspace-id="workspaceId"
      @update:open="(open: boolean) => (editOpen = open)"
      @saved="onMetadataSaved"
    />
  </div>
</template>
