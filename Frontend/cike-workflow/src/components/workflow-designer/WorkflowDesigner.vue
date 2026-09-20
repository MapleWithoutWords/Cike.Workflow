<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from "vue"
import { Redo2, Trash2, Undo2 } from "@lucide/vue"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog"
import { Button } from "@/components/ui/button"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import type { DesignerCommand } from "@/core/designer/commands"
import DesignerBreadcrumb from "./DesignerBreadcrumb.vue"
import DesignerCanvas from "./DesignerCanvas.vue"
import ActivityPalette from "./ActivityPalette.vue"
import PropertyPanel from "./PropertyPanel.vue"

const props = defineProps<{ designer: WorkflowDesignerState }>()

const selectedForPanel = computed(() => {
  const activity = props.designer.selectedActivity.value
  if (!activity) return null
  return { name: activity.name ?? activity.type, type: activity.type }
})

const entryKey = computed(() => {
  const stack = props.designer.drillStack.value
  return stack.map((entry) => entry.activity.id).join("/")
})

const removeDialogOpen = ref(false)
const pendingRemoveCommand = ref<DesignerCommand | null>(null)

function requestRemove(activityId: string): void {
  const command = props.designer.buildRemoveCommand(activityId)
  if (!command) return
  pendingRemoveCommand.value = command
  removeDialogOpen.value = true
}

function confirmRemove(): void {
  const command = pendingRemoveCommand.value
  pendingRemoveCommand.value = null
  if (!command) return
  props.designer.executeCommand(command)
  if (props.designer.selectedActivityId.value === null) return
}

function drillById(activityId: string): void {
  const activity = props.designer.currentChildren.value.find((child) => child.id === activityId)
  if (activity) props.designer.drillInto(activity)
}

function onKeydown(event: KeyboardEvent): void {
  if (event.key !== "Delete" && event.key !== "Backspace") return
  const target = event.target as HTMLElement | null
  if (target && (target.tagName === "INPUT" || target.tagName === "TEXTAREA" || target.isContentEditable)) return
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
        <DesignerBreadcrumb :entries="designer.breadcrumb.value" @select="(index: number) => designer.popTo(index)" />
        <span
          v-if="designer.missingStartNode.value"
          class="shrink-0 rounded bg-warning/15 px-1.5 py-0.5 text-xs text-warning"
        >
          画布缺少开始节点，发布前需添加
        </span>
      </div>
      <div class="flex shrink-0 items-center gap-2">
        <Button variant="ghost" size="icon" :disabled="!designer.canUndo.value" title="撤销" @click="designer.undo()">
          <Undo2 :size="16" />
        </Button>
        <Button variant="ghost" size="icon" :disabled="!designer.canRedo.value" title="重做" @click="designer.redo()">
          <Redo2 :size="16" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          :disabled="!designer.selectedActivityId.value"
          title="删除节点"
          @click="designer.selectedActivityId.value && requestRemove(designer.selectedActivityId.value)"
        >
          <Trash2 :size="16" />
        </Button>
        <Button size="sm" :disabled="designer.saving.value" @click="designer.save()">
          {{ designer.saving.value ? "保存中…" : "保存" }}
        </Button>
      </div>
    </header>
    <div class="flex min-h-0 flex-1">
      <ActivityPalette />
      <div class="relative min-w-0 flex-1">
        <DesignerCanvas
          :projection="designer.projection.value"
          :interactive="true"
          :selected-id="designer.selectedActivityId.value"
          :entry-key="entryKey"
          :entry-activity="designer.currentEntry.value?.activity ?? null"
          @node-click="(id: string) => (designer.selectedActivityId.value = id || null)"
          @node-dblclick="(id: string) => drillById(id)"
          @node-moved="(payload) => designer.moveNode(payload)"
          @viewport-changed="(state) => designer.saveViewport(state)"
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
      <PropertyPanel :selected="selectedForPanel" />
    </div>

    <AlertDialog :open="removeDialogOpen" @update:open="(open: boolean) => (removeDialogOpen = open)">
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>删除节点</AlertDialogTitle>
          <AlertDialogDescription>
            <template v-if="(pendingRemoveCommand?.removedConnectionCount ?? 0) > 0">
              将同时移除该节点关联的 {{ pendingRemoveCommand?.removedConnectionCount }} 条连线。
            </template>
            <template v-else>确认删除选中的节点？</template>
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>取消</AlertDialogCancel>
          <AlertDialogAction @click="confirmRemove">删除</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  </div>
</template>
