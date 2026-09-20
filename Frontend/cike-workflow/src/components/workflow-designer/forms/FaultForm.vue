<script setup lang="ts">
import { Input as UiInput } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { makeEditPropertyCommand } from "@/core/designer/commands"

const props = defineProps<{ activity: unknown; designer: { executeCommand: (command: unknown) => void } }>()

const FIELDS: Array<{ key: string; label: string }> = [
  { key: "faultCode", label: "故障码" },
  { key: "category", label: "类别" },
  { key: "faultType", label: "故障类型" },
  { key: "message", label: "消息" },
]

const activity = props.activity as unknown as Record<string, { expression: { value?: unknown } }>

function field(key: string) {
  return activity[key].expression as unknown as Record<string, unknown>
}

function commit(key: string, event: Event): void {
  const raw = (event.target as HTMLInputElement).value
  const from = field(key)["value"]
  const to = raw === "" ? null : raw
  props.designer.executeCommand(makeEditPropertyCommand(field(key), "value", from, to))
}
</script>

<template>
  <div class="space-y-2">
    <div v-for="item in FIELDS" :key="item.key" class="space-y-1">
      <Label class="text-xs">{{ item.label }}</Label>
      <UiInput :model-value="String(field(item.key)['value'] ?? '')" @change="commit(item.key, $event)" />
    </div>
  </div>
</template>
