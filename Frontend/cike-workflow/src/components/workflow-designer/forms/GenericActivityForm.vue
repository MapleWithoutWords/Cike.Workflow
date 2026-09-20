<script setup lang="ts">
import { computed } from "vue"
import { Input as UiInput } from "@/components/ui/input"
import { Switch } from "@/components/ui/switch"
import { Textarea } from "@/components/ui/textarea"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { IActivity } from "@/core/abstracts/Activity"
import { makeEditPropertyCommand } from "@/core/designer/commands"
import type { DesignerCommand } from "@/core/designer/commands"
import { resolveInputFields, type ResolvedInputField } from "@/core/designer/form"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"

const props = defineProps<{
  activity: IActivity
  descriptors: { name?: string; clrName?: string; displayName?: string | null; description?: string | null; isReadOnly?: boolean | null }[]
  designer: WorkflowDesignerState
}>()

const fields = computed(() => resolveInputFields(props.activity, props.descriptors))

function valueOf(field: ResolvedInputField): string {
  const raw = field.input?.expression.value
  if (raw == null) return ""
  return typeof raw === "object" ? JSON.stringify(raw) : String(raw)
}

function commitLiteral(field: ResolvedInputField, raw: string): void {
  if (!field.input) return
  const expression = field.input.expression
  const from = expression.value
  let to: unknown = raw
  if (typeof from === "number") to = raw === "" ? null : Number(raw)
  else if (typeof from === "boolean") to = raw === "true"
  else if (typeof from === "object" && from !== null) {
    try {
      to = raw ? JSON.parse(raw) : null
    } catch {
      return
    }
  } else if (raw === "") to = null
  props.designer.executeCommand(editValueCommand(field, to, from))
}

function commitBoolean(field: ResolvedInputField, to: boolean): void {
  if (!field.input) return
  props.designer.executeCommand(editValueCommand(field, to, field.input.expression.value))
}

function commitExpressionType(field: ResolvedInputField, type: string): void {
  if (!field.input) return
  const expression = field.input.expression
  const from = expression.type
  props.designer.executeCommand(
    makeEditPropertyCommand(expression as Record<string, unknown>, "type", from, type),
  )
}

function editValueCommand(field: ResolvedInputField, to: unknown, from: unknown): DesignerCommand {
  return makeEditPropertyCommand(field.input!.expression as unknown as Record<string, unknown>, "value", from, to)
}

function isBooleanField(field: ResolvedInputField): boolean {
  return typeof field.input?.expression.value === "boolean"
}

function isNumberField(field: ResolvedInputField): boolean {
  return typeof field.input?.expression.value === "number"
}

function isObjectField(field: ResolvedInputField): boolean {
  const value = field.input?.expression.value
  return value != null && typeof value === "object"
}
</script>

<template>
  <div class="space-y-3">
    <div v-for="field in fields" :key="field.field" class="space-y-1">
      <Label class="text-xs" :title="field.description ?? undefined">{{ field.label }}</Label>
      <div v-if="field.readOnly" class="text-xs text-muted-foreground">只读</div>
      <template v-else-if="field.input">
        <Switch
          v-if="isBooleanField(field)"
          :model-value="field.input.expression.value === true"
          @update:model-value="(checked: boolean) => commitBoolean(field, checked)"
        />
        <UiInput
          v-else-if="isNumberField(field)"
          type="number"
          :model-value="valueOf(field)"
          @change="(event: Event) => commitLiteral(field, (event.target as HTMLInputElement).value)"
        />
        <Textarea
          v-else-if="isObjectField(field)"
          class="font-mono text-xs"
          rows="4"
          :model-value="valueOf(field)"
          @change="(event: Event) => commitLiteral(field, (event.target as HTMLTextAreaElement).value)"
        />
        <UiInput
          v-else
          :model-value="valueOf(field)"
          @change="(event: Event) => commitLiteral(field, (event.target as HTMLInputElement).value)"
        />
        <div class="flex items-center gap-1.5">
          <Select
            :model-value="field.input.expression.type"
            @update:model-value="(type) => commitExpressionType(field, String(type))"
          >
            <SelectTrigger class="h-6 w-32 text-xs">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="Literal">字面量</SelectItem>
              <SelectItem value="JavaScript">JavaScript</SelectItem>
              <SelectItem value="CSharp">C#</SelectItem>
              <SelectItem value="Liquid">Liquid</SelectItem>
            </SelectContent>
          </Select>
          <span class="text-[10px] text-muted-foreground">表达式类型</span>
        </div>
      </template>
      <div v-else class="font-mono text-xs text-muted-foreground">{{ String((activity as unknown as Record<string, unknown>)[field.field] ?? "—") }}</div>
    </div>
  </div>
</template>
