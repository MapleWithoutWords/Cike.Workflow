<script setup lang="ts">
import { PanelLeftClose, PanelLeftOpen } from "@lucide/vue"
import type { PaletteGroup } from "@/core/designer/palette"
import { useDockState } from "@/composables/useDockState"
import DockPinButton from "./DockPinButton.vue"
import { resolveActivityIcon } from "./nodes/icons"

defineProps<{ groups: PaletteGroup[] }>()

const dock = useDockState()
/** Collapsible like an IDE side panel: collapsed leaves a thin rail to reopen. */
const open = dock.open.palette

const emit = defineEmits<{
  add: [typeName: string]
  dragstart: [payload: { typeName: string; event: DragEvent }]
}>()

function onDragStart(typeName: string, event: DragEvent): void {
  event.dataTransfer?.setData("cike-activity-type", typeName)
  event.dataTransfer?.setData("text/plain", typeName)
  emit("dragstart", { typeName, event })
}
</script>

<template>
  <aside v-if="open" class="flex w-56 shrink-0 flex-col overflow-y-auto border-r">
    <div class="sticky top-0 flex items-center border-b bg-background px-3 py-2 text-xs font-medium text-muted-foreground">
      活动
      <DockPinButton panel="palette" class="ml-auto p-0.5" />
      <button
        type="button"
        class="rounded p-0.5 hover:bg-muted hover:text-foreground"
        title="收起活动面板"
        @click="dock.setOpen('palette', false)"
      >
        <PanelLeftClose :size="14" />
      </button>
    </div>
    <div v-if="groups.length === 0" class="px-3 py-6 text-center text-xs text-muted-foreground">活动清单加载中…</div>
    <div v-for="group in groups" :key="group.category" class="border-b last:border-b-0">
      <div class="px-3 pb-1 pt-3 text-[10px] font-medium uppercase tracking-wide text-muted-foreground">
        {{ group.category }}
      </div>
      <button
        v-for="item in group.items"
        :key="item.typeName"
        class="flex w-full items-center gap-2 px-3 py-1.5 text-left transition-colors"
        :class="item.canAdd ? 'cursor-grab hover:bg-accent' : 'cursor-not-allowed opacity-50'"
        :draggable="item.canAdd"
        :title="item.description ?? item.typeName"
        @click="item.canAdd && emit('add', item.typeName)"
        @dragstart="item.canAdd && onDragStart(item.typeName, $event)"
      >
        <component :is="resolveActivityIcon(item.icon)" :size="15" class="shrink-0 text-muted-foreground" />
        <div class="min-w-0">
          <div class="truncate text-xs font-medium text-foreground">{{ item.displayName }}</div>
          <div v-if="!item.canAdd" class="text-[10px] text-muted-foreground">前端未镜像，不可添加</div>
        </div>
      </button>
    </div>
  </aside>
  <aside v-else class="flex w-9 shrink-0 flex-col items-center border-r bg-background py-1">
    <button
      type="button"
      class="rounded p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
      title="展开活动面板"
      @click="dock.setOpen('palette', true)"
    >
      <PanelLeftOpen :size="15" />
    </button>
  </aside>
</template>
