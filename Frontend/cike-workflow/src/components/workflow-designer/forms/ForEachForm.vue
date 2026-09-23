<script setup lang="ts">
import { computed } from "vue"
import { Textarea } from "@/components/ui/textarea"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import type { ExpressionLike } from "@/core/designer/expression"
import { coerceLiteralValue } from "@/core/designer/form"
import ExpressionEditor from "../ExpressionEditor.vue"

const props = defineProps<{ activity: unknown; designer: WorkflowDesignerState }>()

const items = computed<ExpressionLike>(
  () => (props.activity as unknown as { items: { expression: ExpressionLike } }).items.expression,
)

function text(value: unknown): string {
  return value == null ? "" : JSON.stringify(value)
}
</script>

<template>
  <ExpressionEditor :expression="items" :designer="designer" label="迭代集合" :literal-default="[]">
    <template #default="{ value, commit }">
      <Textarea
        class="font-mono text-xs"
        rows="5"
        :model-value="text(value)"
        @change="(e: Event) => { const to = coerceLiteralValue(value, (e.target as HTMLTextAreaElement).value); if (to !== undefined) commit(to) }"
      />
      <div class="text-[10px] text-muted-foreground">JSON 数组，如 [1, 2, 3] 或表达式返回的集合</div>
    </template>
  </ExpressionEditor>
</template>
