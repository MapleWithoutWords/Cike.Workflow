<script setup lang="ts">
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import type { If } from "@/core/activities/If"
import { makeEditPropertyCommand } from "@/core/designer/commands"

const props = defineProps<{ activity: unknown; designer: { executeCommand: (command: unknown) => void } }>()

const condition = () => (props.activity as If).condition

function commit(value: boolean): void {
  const expression = condition().expression as unknown as Record<string, unknown>
  props.designer.executeCommand(makeEditPropertyCommand(expression, "value", expression["value"], value))
}
</script>

<template>
  <div class="space-y-1">
    <Label class="text-xs">条件</Label>
    <div class="flex items-center gap-2">
      <Switch :model-value="condition().expression.value === true" @update:model-value="commit" />
      <span class="text-xs text-muted-foreground">{{ condition().expression.value === true ? "True" : "False" }}</span>
    </div>
  </div>
</template>
