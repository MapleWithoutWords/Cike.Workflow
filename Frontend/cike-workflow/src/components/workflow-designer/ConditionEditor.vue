<script setup lang="ts">
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import { compileGroup, type ConditionGroup, type ConditionSpec } from "@/core/designer/conditionCompile"
import { emptyGroup } from "@/core/designer/conditionModel"
import ConditionGroupEditor from "./ConditionGroupEditor.vue"
import ExpressionEditor from "./ExpressionEditor.vue"

/**
 * Top-level editor for one condition (If's condition / one Switch case).
 * The stored discriminator `type` decides the editor in a single read (ADR 0010):
 *   custom    → the visual builder (ConditionGroupEditor)
 *   otherwise → the spec itself is handed to the unified ExpressionEditor as an
 *               escape hatch, whitelisted to Literal / Javascript / Liquid, with
 *               the Literal slot rendering a true/false switch.
 * The top select is the custom gate (default custom); ExpressionEditor's icon
 * switcher moves among the three escape-hatch types. Structural edits (entering /
 * leaving custom) emit a controlled change; in-place value/type edits are owned by
 * ExpressionEditor. The compiled wire expression is a derived projection synced by
 * the owning form on revision (ADR 0010), not written here.
 */
const props = defineProps<{
  spec: ConditionSpec
  designer: WorkflowDesignerState
  readonly?: boolean
  label?: string
}>()
const emit = defineEmits<{ change: [ConditionSpec] }>()

const TYPE_LABELS: Record<ConditionSpec["type"], string> = {
  custom: "可视化",
  Literal: "字面量",
  Javascript: "JavaScript",
  Liquid: "Liquid",
}

const ESCAPE_TYPES = ["Literal", "Javascript", "Liquid"]

function onTypeChange(target: ConditionSpec["type"]): void {
  if (target === props.spec.type) return
  const current = props.spec
  let value: ConditionSpec["value"]
  if (target === "custom") {
    value = current.type === "custom" ? (current.value as ConditionGroup) : emptyGroup()
  } else if (target === "Literal") {
    value = current.type === "Literal" ? current.value === true : false
  } else if (target === "Javascript") {
    // Leaving the builder compiles the tree to code so semantics survive the switch.
    value = current.type === "custom" ? compileGroup(current.value as ConditionGroup) : typeof current.value === "string" ? current.value : ""
  } else {
    value = typeof current.value === "string" ? current.value : ""
  }
  emit("change", { type: target, value })
}

function onGroupChange(group: ConditionGroup): void {
  emit("change", { type: "custom", value: group })
}
</script>

<template>
  <div class="space-y-1.5">
    <div class="flex items-center gap-2">
      <Label v-if="label" class="text-xs">{{ label }}</Label>
      <Select
        class="ml-auto"
        :model-value="spec.type"
        :disabled="readonly"
        @update:model-value="(t) => onTypeChange(t as ConditionSpec['type'])"
      >
        <SelectTrigger size="sm" class="h-7 w-28 text-xs">
          <SelectValue class="block! min-w-0 truncate" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem v-for="(text, value) in TYPE_LABELS" :key="value" :value="value" class="text-xs">
            {{ text }}
          </SelectItem>
        </SelectContent>
      </Select>
    </div>

    <ConditionGroupEditor
      v-if="spec.type === 'custom'"
      :group="(spec.value as ConditionGroup) ?? emptyGroup()"
      :designer="designer"
      :readonly="readonly"
      @change="onGroupChange"
    />

    <ExpressionEditor
      v-else
      :expression="spec"
      :designer="designer"
      :allowed-types="ESCAPE_TYPES"
      :literal-default="false"
      :readonly="readonly"
    >
      <template #default="{ value, commit, readonly: ro }">
        <div class="flex h-8 items-center gap-2">
          <Switch :model-value="value === true" :disabled="ro" @update:model-value="(v: boolean) => commit(v)" />
          <span class="text-xs text-muted-foreground">{{ value === true ? "True" : "False" }}</span>
        </div>
      </template>
    </ExpressionEditor>
  </div>
</template>
