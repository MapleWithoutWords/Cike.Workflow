<script setup lang="ts">
import { computed } from "vue"
import type { Node } from "@antv/x6"
import {
  AlertTriangle,
  Boxes,
  FileCode,
  GitFork,
  Globe,
  LogOut,
  Play,
  Repeat,
  RotateCw,
  Split,
  Square,
  Workflow,
} from "@lucide/vue"
import type { DesignerNodeData } from "@/core/designer/projection"

const props = defineProps<{ node: Node }>()

const data = computed(() => props.node.getData() as DesignerNodeData & { selected?: boolean })

const ICONS: Record<string, unknown> = {
  Start: Play,
  End: Square,
  Fault: AlertTriangle,
  Break: LogOut,
  If: GitFork,
  Switch: Split,
  While: RotateCw,
  For: Repeat,
  ForEach: Repeat,
  Flowchart: Workflow,
  RunJavaScript: FileCode,
  SendHttpRequest: Globe,
}

const icon = computed(() => (ICONS[data.value.typeShort] ?? Boxes) as unknown)
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
