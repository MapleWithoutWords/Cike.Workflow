<script setup lang="ts">
import { computed } from "vue"
import type { Node } from "@antv/x6"
import type { DesignerNodeData } from "@/core/designer/projection"
import { resolveActivityIcon } from "./icons"

const props = defineProps<{ node: Node }>()

const data = computed(() => props.node.getData() as DesignerNodeData & { selected?: boolean })

const icon = computed(() => resolveActivityIcon(data.value.icon))
</script>

<template>
  <div
    class="flex h-full w-full items-center gap-2 rounded-md border bg-background px-3 shadow-sm transition-shadow"
    :class="[
      data.isGeneric ? 'border-dashed' : '',
      data.selected ? 'border-primary ring-2 ring-primary' : '',
    ]"
  >
    <component :is="icon" :size="14" class="shrink-0 text-muted-foreground" />
    <span class="min-w-0 truncate text-xs font-medium text-foreground">{{ data.name }}</span>
    <span v-if="data.isGeneric" class="ml-auto shrink-0 text-[10px] text-muted-foreground">{{ data.typeShort }}</span>
  </div>
</template>
