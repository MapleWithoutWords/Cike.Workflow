<script setup lang="ts">
import { computed } from "vue"
import type { Node } from "@antv/x6"
import { CircleAlert } from "@lucide/vue"
import type { DesignerNodeData } from "@/core/designer/projection"
import { ACTIVITY_STATUS_UI } from "@/core/designer/execution"
import { resolveActivityIcon } from "./icons"

const props = defineProps<{ node: Node }>()

const data = computed(() => props.node.getData() as DesignerNodeData & { selected?: boolean })

const icon = computed(() => resolveActivityIcon(data.value.icon))

const statusUi = computed(() => (data.value.status != null ? ACTIVITY_STATUS_UI[data.value.status] : null))
</script>

<template>
  <div
    class="flex h-full w-full items-center gap-2 rounded-md border bg-card px-3 text-card-foreground shadow-sm transition-shadow"
    :class="[
      data.isGeneric ? 'border-dashed' : '',
      data.selected
        ? 'border-primary ring-2 ring-primary/40'
        : data.hasError
          ? 'border-destructive ring-1 ring-destructive/35'
          : '',
    ]"
  >
    <component :is="icon" :size="14" class="shrink-0 text-muted-foreground" />
    <span class="min-w-0 truncate text-xs font-medium text-foreground">{{ data.name }}</span>
    <CircleAlert
      v-if="data.hasError"
      :size="14"
      class="ml-auto shrink-0 text-destructive"
      aria-label="校验问题"
    />
    <span
      v-else-if="statusUi"
      class="ml-auto shrink-0 rounded px-1.5 py-0.5 text-[10px] font-medium"
      :class="statusUi.class"
    >{{ statusUi.label }}</span>
    <span v-else-if="data.isGeneric" class="ml-auto shrink-0 text-[10px] text-muted-foreground">{{ data.typeShort }}</span>
  </div>
</template>
