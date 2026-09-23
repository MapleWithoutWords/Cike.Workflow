<script setup lang="ts">
import { computed } from "vue"
import { Input as UiInput } from "@/components/ui/input"
import { Switch } from "@/components/ui/switch"
import { Textarea } from "@/components/ui/textarea"
import { Label } from "@/components/ui/label"
import type { IActivity } from "@/core/abstracts/Activity"
import { resolveInputFields, type ResolvedInputField } from "@/core/designer/form"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import ExpressionEditor from "../ExpressionEditor.vue"

const props = defineProps<{
  activity: IActivity
  descriptors: { name?: string; clrName?: string; displayName?: string | null; description?: string | null; isReadOnly?: boolean | null; defaultValue?: unknown }[]
  designer: WorkflowDesignerState
}>()

const fields = computed(() => resolveInputFields(props.activity, props.descriptors))

/** Concrete zero/default for the Literal reset (ADR 0005): backend default when
 *  present, else a fresh type-zero inferred from the current value's shape.
 *  Never returns bare null and never reuses the previous value's reference. */
function literalDefaultFor(field: ResolvedInputField): unknown {
  if (field.defaultValue !== undefined && field.defaultValue !== null) return field.defaultValue
  const value = field.input?.expression.value
  if (Array.isArray(value)) return []
  if (value != null && typeof value === "object") return {}
  if (typeof value === "number") return 0
  if (typeof value === "boolean") return false
  return ""
}

function asText(value: unknown): string {
  if (value == null) return ""
  return typeof value === "object" ? JSON.stringify(value) : String(value)
}

function coerceNumber(raw: string): unknown {
  return raw === "" ? null : Number(raw)
}

function coerceJson(raw: string): unknown {
  if (!raw) return null
  try {
    return JSON.parse(raw)
  } catch {
    return undefined
  }
}
</script>

<template>
  <div class="space-y-3">
    <div v-for="field in fields" :key="field.field" class="space-y-1">
      <ExpressionEditor
        v-if="field.input && !field.readOnly"
        :expression="field.input.expression"
        :designer="designer"
        :label="field.label"
        :description="field.description"
        :literal-default="literalDefaultFor(field)"
      >
        <template #default="{ value, commit }">
          <Switch
            v-if="typeof value === 'boolean'"
            :model-value="value === true"
            @update:model-value="(checked: boolean) => commit(checked)"
          />
          <UiInput
            v-else-if="typeof value === 'number'"
            type="number"
            :model-value="asText(value)"
            @change="(event: Event) => commit(coerceNumber((event.target as HTMLInputElement).value))"
          />
          <Textarea
            v-else-if="value != null && typeof value === 'object'"
            class="font-mono text-xs"
            rows="4"
            :model-value="asText(value)"
            @change="(event: Event) => { const to = coerceJson((event.target as HTMLTextAreaElement).value); if (to !== undefined) commit(to) }"
          />
          <UiInput
            v-else
            :model-value="asText(value)"
            @change="(event: Event) => { const raw = (event.target as HTMLInputElement).value; commit(raw === '' ? null : raw) }"
          />
        </template>
      </ExpressionEditor>

      <template v-else>
        <Label class="text-xs" :title="field.description ?? undefined">{{ field.label }}</Label>
        <div v-if="field.readOnly" class="text-xs text-muted-foreground">只读</div>
        <div v-else class="font-mono text-xs text-muted-foreground">{{ String((activity as unknown as Record<string, unknown>)[field.field] ?? "—") }}</div>
      </template>
    </div>
  </div>
</template>
