<script setup lang="ts">
import { Input as UiInput } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Label } from "@/components/ui/label"
import { makeEditPropertyCommand } from "@/core/designer/commands"
import { coerceLiteralValue } from "@/core/designer/form"

const props = defineProps<{ activity: unknown; designer: { executeCommand: (command: unknown) => void } }>()

const activity = props.activity as unknown as Record<string, { expression: { value?: unknown } }>

function field(key: string) {
  return activity[key].expression as unknown as Record<string, unknown>
}

function display(key: string): string {
  const value = field(key)["value"]
  if (key === "possibleOutcomes") {
    const outcomes = value as unknown
    return Array.isArray(outcomes) ? outcomes.join(", ") : ""
  }
  return value == null ? "" : typeof value === "object" ? JSON.stringify(value) : String(value)
}

function commit(key: string, event: Event): void {
  const raw = (event.target as HTMLInputElement | HTMLTextAreaElement).value
  const from = field(key)["value"]
  const to = key === "possibleOutcomes"
    ? raw.split(",").map((entry) => entry.trim()).filter(Boolean)
    : coerceLiteralValue(from, raw)
  if (to === undefined) return
  props.designer.executeCommand(makeEditPropertyCommand(field(key), "value", from, to))
}
</script>

<template>
  <div class="space-y-2">
    <div class="space-y-1">
      <Label class="text-xs">脚本</Label>
      <Textarea class="min-h-28 font-mono text-xs" :model-value="display('script')" @change="commit('script', $event)" />
    </div>
    <div class="space-y-1">
      <Label class="text-xs">可能出端口（逗号分隔）</Label>
      <UiInput :model-value="display('possibleOutcomes')" @change="commit('possibleOutcomes', $event)" />
    </div>
  </div>
</template>
