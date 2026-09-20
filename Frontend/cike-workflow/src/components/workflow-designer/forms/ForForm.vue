<script setup lang="ts">
import { Input as UiInput } from "@/components/ui/input"
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import type { For } from "@/core/activities/For"
import { makeEditPropertyCommand } from "@/core/designer/commands"

const props = defineProps<{ activity: unknown; designer: { executeCommand: (command: unknown) => void } }>()

const fields = (props.activity as For) as unknown as Record<string, { expression: { value?: unknown } }>

function commitNumber(key: string, event: Event): void {
  const expression = fields[key].expression as unknown as Record<string, unknown>
  const from = expression["value"]
  const raw = (event.target as HTMLInputElement).value
  const to = raw === "" ? null : Number(raw)
  props.designer.executeCommand(makeEditPropertyCommand(expression, "value", from, to))
}

function commitInclusive(value: boolean): void {
  const expression = fields["outerBoundInclusive"].expression as unknown as Record<string, unknown>
  props.designer.executeCommand(makeEditPropertyCommand(expression, "value", expression["value"], value))
}
</script>

<template>
  <div class="space-y-2">
    <div class="space-y-1">
      <Label class="text-xs">起始值</Label>
      <UiInput type="number" :model-value="String(fields['start'].expression.value ?? 0)" @change="commitNumber('start', $event)" />
    </div>
    <div class="space-y-1">
      <Label class="text-xs">结束值</Label>
      <UiInput type="number" :model-value="String(fields['end'].expression.value ?? 0)" @change="commitNumber('end', $event)" />
    </div>
    <div class="space-y-1">
      <Label class="text-xs">步长</Label>
      <UiInput type="number" :model-value="String(fields['step'].expression.value ?? 1)" @change="commitNumber('step', $event)" />
    </div>
    <div class="space-y-1">
      <Label class="text-xs">含右边界</Label>
      <Switch :model-value="fields['outerBoundInclusive'].expression.value === true" @update:model-value="commitInclusive" />
    </div>
  </div>
</template>
