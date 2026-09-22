<script setup lang="ts">
import { computed } from "vue"
import { PanelRightClose, PanelRightOpen } from "@lucide/vue"
import { Input as UiInput } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import { useDockState } from "@/composables/useDockState"
import DockPinButton from "./DockPinButton.vue"
import type { IActivity } from "@/core/abstracts/Activity"
import { makeEditPropertyCommand } from "@/core/designer/commands"
import { activityShortName } from "@/core/designer/registry"
import { getMergeMode, MERGE_MODES, setMergeMode } from "@/core/designer/form"
import { FORM_REGISTRY } from "./forms"
import GenericActivityForm from "./forms/GenericActivityForm.vue"

const props = defineProps<{
  activity: IActivity | null
  designer: WorkflowDesignerState
}>()

const dock = useDockState()
/** Collapsible like an IDE side panel: collapsed leaves a thin rail to reopen. */
const open = dock.open.property

const typeShort = computed(() => (props.activity ? activityShortName(props.activity.type) : ""))

const dedicatedForm = computed(() => FORM_REGISTRY[typeShort.value] ?? null)

const descriptors = computed(() => {
  if (!props.activity) return []
  const map = props.designer.descriptorByType.value
  const descriptor = map.get(props.activity.type) ?? map.get(`Cike.${typeShort.value}`) ?? null
  return descriptor?.inputs ?? []
})

const inboundCount = computed(() => {
  if (!props.activity) return 0
  return props.designer.connectionTargets.value.get(props.activity.id) ?? 0
})

const currentMergeMode = computed(() => (props.activity ? getMergeMode(props.activity) : null))

function commitName(value: string): void {
  const activity = props.activity
  if (!activity) return
  const from = activity.name ?? null
  const to = value === "" ? null : value
  props.designer.executeCommand(
    makeEditPropertyCommand(activity as unknown as Record<string, unknown>, "name", from, to),
  )
}

function commitMergeMode(mode: string): void {
  const activity = props.activity
  if (!activity) return
  const from = getMergeMode(activity)
  setMergeMode(activity as IActivity, mode)
  props.designer.executeCommand(
    makeEditPropertyCommand(activity.customProperties as Record<string, unknown>, "mergeMode", from, mode),
  )
}
</script>

<template>
  <aside v-if="open" class="flex w-72 shrink-0 flex-col overflow-y-auto border-l">
    <div class="flex items-center border-b px-3 py-2 text-xs font-medium text-muted-foreground">
      属性
      <DockPinButton panel="property" class="ml-auto p-0.5" />
      <button
        type="button"
        class="rounded p-0.5 hover:bg-muted hover:text-foreground"
        title="收起属性面板"
        @click="dock.setOpen('property', false)"
      >
        <PanelRightClose :size="14" />
      </button>
    </div>
    <div v-if="!activity" class="px-3 py-6 text-center text-xs text-muted-foreground">未选中节点</div>
    <div v-else class="space-y-4 px-3 py-3">
      <div class="space-y-1">
        <div class="text-sm font-medium text-foreground">{{ activity.name ?? activityShortName(activity.type) }}</div>
        <div class="font-mono text-xs text-muted-foreground">{{ activity.type }}</div>
      </div>

      <div class="space-y-1">
        <Label class="text-xs">名称</Label>
        <UiInput
          :model-value="activity.name ?? ''"
          :placeholder="activityShortName(activity.type)"
          @change="(event: Event) => commitName((event.target as HTMLInputElement).value)"
        />
      </div>

      <div v-if="inboundCount >= 2" class="space-y-1">
        <Label class="text-xs">合并模式</Label>
        <Select :model-value="currentMergeMode ?? ''" @update:model-value="(mode) => commitMergeMode(String(mode))">
          <SelectTrigger class="h-8 text-xs">
            <SelectValue placeholder="未设置（按 Stream）" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="mode in MERGE_MODES" :key="mode" :value="mode">{{ mode }}</SelectItem>
          </SelectContent>
        </Select>
        <div class="text-[10px] text-muted-foreground">多入边节点的汇聚语义</div>
      </div>

      <component
        :is="dedicatedForm"
        v-if="dedicatedForm"
        :activity="activity"
        :descriptors="descriptors"
        :designer="designer"
      />
      <div v-else class="space-y-2">
        <div class="text-xs font-medium text-muted-foreground">输入属性</div>
        <GenericActivityForm :activity="activity" :descriptors="descriptors" :designer="designer" />
      </div>
    </div>
  </aside>
  <aside v-else class="flex w-9 shrink-0 flex-col items-center border-l bg-background py-1">
    <button
      type="button"
      class="rounded p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
      title="展开属性面板"
      @click="dock.setOpen('property', true)"
    >
      <PanelRightOpen :size="15" />
    </button>
    <DockPinButton panel="property" :size="15" class="p-1" />
  </aside>
</template>
