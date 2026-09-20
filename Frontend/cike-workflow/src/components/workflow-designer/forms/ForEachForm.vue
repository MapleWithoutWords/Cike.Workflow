<script setup lang="ts">
import { Textarea } from "@/components/ui/textarea"
import { Label } from "@/components/ui/label"
import type { ForEach } from "@/core/activities/ForEach"
import { makeEditPropertyCommand } from "@/core/designer/commands"
import { coerceLiteralValue } from "@/core/designer/form"

const props = defineProps<{ activity: unknown; designer: { executeCommand: (command: unknown) => void } }>()

const items = () => ((props.activity as ForEach).items.expression as unknown as Record<string, unknown>)

function display(): string {
  const value = items()["value"]
  return value == null ? "" : JSON.stringify(value)
}

function commit(event: Event): void {
  const raw = (event.target as HTMLTextAreaElement).value
  const to = coerceLiteralValue(items()["value"], raw)
  if (to === undefined) return
  props.designer.executeCommand(makeEditPropertyCommand(items(), "value", items()["value"], to))
}
</script>

<template>
  <div class="space-y-1">
    <Label class="text-xs">迭代集合</Label>
    <Textarea class="font-mono text-xs" rows="5" :model-value="display()" @change="commit" />
    <div class="text-[10px] text-muted-foreground">JSON 数组，如 [1, 2, 3] 或表达式返回的集合</div>
  </div>
</template>
