<script setup lang="ts">
import { computed } from "vue"
import { Input as UiInput } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import type { ExpressionLike } from "@/core/designer/expression"
import { coerceLiteralValue } from "@/core/designer/form"
import ExpressionEditor from "../ExpressionEditor.vue"

const props = defineProps<{ activity: unknown; designer: WorkflowDesignerState }>()

const activity = computed(() => props.activity as unknown as Record<string, { expression: ExpressionLike }>)

function expr(key: string): ExpressionLike {
  return activity.value[key].expression
}

function scriptText(value: unknown): string {
  return value == null ? "" : typeof value === "object" ? JSON.stringify(value) : String(value)
}

function outcomesText(value: unknown): string {
  return Array.isArray(value) ? value.join(", ") : ""
}

function parseOutcomes(raw: string): string[] {
  return raw.split(",").map((entry) => entry.trim()).filter(Boolean)
}
</script>

<template>
  <div class="space-y-2">
    <ExpressionEditor :expression="expr('script')" :designer="designer" label="脚本" literal-default="">
      <template #default="{ value, commit }">
        <Textarea
          class="min-h-28 font-mono text-xs"
          :model-value="scriptText(value)"
          @change="(e: Event) => { const to = coerceLiteralValue(value, (e.target as HTMLTextAreaElement).value); if (to !== undefined) commit(to) }"
        />
      </template>
    </ExpressionEditor>
    <ExpressionEditor :expression="expr('possibleOutcomes')" :designer="designer" label="可能出端口（逗号分隔）" :literal-default="[]">
      <template #default="{ value, commit }">
        <UiInput
          :model-value="outcomesText(value)"
          @change="(e: Event) => commit(parseOutcomes((e.target as HTMLInputElement).value))"
        />
      </template>
    </ExpressionEditor>
  </div>
</template>
