<script setup lang="ts">
import { computed } from "vue"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import type { IActivity } from "@/core/abstracts/Activity"
import { compileCondition, type ConditionSpec } from "@/core/designer/conditionCompile"
import { readIfCondition, type CustomExpressionContainer } from "@/core/designer/conditionModel"
import { makeBatchCommand, makeEditPropertyCommand } from "@/core/designer/commands"
import { makeSetExpressionCommand } from "@/core/designer/expression"
import ConditionEditor from "../ConditionEditor.vue"

const props = defineProps<{ activity: unknown; designer: WorkflowDesignerState }>()

interface IfActivity extends IActivity {
  condition: { expression: { type: string; value?: unknown } }
}

const act = computed(() => props.activity as IfActivity)

// Reading revision re-runs this projection after any command / undo (the model
// lives in shallowRefs, so nested plain objects are not reactive on their own).
const spec = computed<ConditionSpec>(() => {
  void props.designer.revision.value
  return readIfCondition(act.value)
})

/**
 * One edit = one undoable command writing BOTH the editing truth
 * (customProperties.customExpression.ifCondition) and the compiled expression
 * (If.condition.expression), so Ctrl+Z never leaves a half-state (ADR 0010).
 */
function onChange(next: ConditionSpec): void {
  const activity = act.value
  const customProperties = activity.customProperties as Record<string, unknown>
  const fromContainer = customProperties["customExpression"] ?? null
  const nextContainer: CustomExpressionContainer = {
    ...(fromContainer as CustomExpressionContainer | null),
    ifCondition: next,
  }
  props.designer.executeCommand(
    makeBatchCommand("修改条件", [
      makeEditPropertyCommand(customProperties, "customExpression", fromContainer, nextContainer),
      makeSetExpressionCommand(activity.condition.expression, compileCondition(next)),
    ]),
  )
}
</script>

<template>
  <ConditionEditor :spec="spec" :readonly="designer.readonly.value" label="条件" @change="onChange" />
</template>
