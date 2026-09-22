<script setup lang="ts">
import { computed } from "vue"
import { Pin, PinOff } from "@lucide/vue"
import { useDockState, type DockPanelId } from "@/composables/useDockState"

const props = defineProps<{ panel: DockPanelId; size?: number }>()

const dock = useDockState()
const pinned = computed(() => dock.pinned[props.panel].value)

const LABELS: Record<DockPanelId, string> = {
  palette: "活动面板",
  property: "属性面板",
  problems: "问题清单",
}
const title = computed(() => `${pinned.value ? "取消固定" : "固定"}${LABELS[props.panel]}`)
</script>

<template>
  <button
    type="button"
    class="rounded hover:bg-muted hover:text-foreground"
    :class="pinned ? 'text-foreground' : 'text-muted-foreground'"
    :title="title"
    @click="dock.togglePin(props.panel)"
  >
    <component :is="pinned ? Pin : PinOff" :size="props.size ?? 14" />
  </button>
</template>
