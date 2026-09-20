<script setup lang="ts">
import type { PaletteGroup } from "@/core/designer/palette"

defineProps<{ groups: PaletteGroup[] }>()

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
  <aside class="flex w-56 shrink-0 flex-col overflow-y-auto border-r">
    <div class="sticky top-0 border-b bg-background px-3 py-2 text-xs font-medium text-muted-foreground">活动</div>
    <div v-if="groups.length === 0" class="px-3 py-6 text-center text-xs text-muted-foreground">活动清单加载中…</div>
    <div v-for="group in groups" :key="group.category" class="border-b last:border-b-0">
      <div class="px-3 pb-1 pt-3 text-[10px] font-medium uppercase tracking-wide text-muted-foreground">
        {{ group.category }}
      </div>
      <button
        v-for="item in group.items"
        :key="item.typeName"
        class="block w-full px-3 py-1.5 text-left transition-colors"
        :class="item.canAdd ? 'cursor-grab hover:bg-accent' : 'cursor-not-allowed opacity-50'"
        :draggable="item.canAdd"
        :title="item.description ?? item.typeName"
        @click="item.canAdd && emit('add', item.typeName)"
        @dragstart="item.canAdd && onDragStart(item.typeName, $event)"
      >
        <div class="text-xs font-medium text-foreground">{{ item.displayName }}</div>
        <div v-if="!item.canAdd" class="text-[10px] text-muted-foreground">前端未镜像，不可添加</div>
      </button>
    </div>
  </aside>
</template>
