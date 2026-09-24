<script setup lang="ts">
import { computed } from "vue"
import { Button } from "@/components/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Plus, Trash2 } from "@lucide/vue"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import {
  OPERATORS_BY_DATATYPE,
  type ConditionComparison,
  type ConditionDataType,
  type ConditionGroup,
  type ConditionOperator,
  type ConditionOperand,
} from "@/core/designer/conditionCompile"
import { emptyGroup } from "@/core/designer/conditionModel"
import ConditionOperandEditor from "./ConditionOperandEditor.vue"

/**
 * One condition group (recursive). Layout contract (ADR 0010): a fixed header row
 * [junction 且/或][+ 添加规则][+ 组合条件] always on one line, then a body whose
 * comparison rows and the indented dashed subgroup sit to the right of a vertical
 * rail descending from the junction's centre. The junction is a narrow ghost select,
 * identical at every depth. Controlled: emits a new group; owns no command logic.
 */
defineOptions({ name: "ConditionGroupEditor" })

const props = defineProps<{ group: ConditionGroup; designer: WorkflowDesignerState; readonly?: boolean; nested?: boolean }>()
const emit = defineEmits<{ change: [ConditionGroup] }>()

const OPERATOR_LABELS: Record<ConditionOperator, string> = {
  "=": "等于",
  "!=": "不等于",
  ">": "大于",
  ">=": "大于等于",
  "<": "小于",
  "<=": "小于等于",
  contains: "包含",
  notContains: "不包含",
  startsWith: "开头是",
  endsWith: "结尾是",
  empty: "为空",
  notEmpty: "不为空",
}

function isUnary(op: ConditionOperator): boolean {
  return op === "empty" || op === "notEmpty"
}

/** The comparison's effective dataType: right literal → left literal → string (ADR 0010). */
function dataTypeOf(cmp: ConditionComparison): ConditionDataType {
  if (cmp.right?.type === "Literal" && cmp.right.dataType) return cmp.right.dataType
  if (cmp.left?.type === "Literal" && cmp.left.dataType) return cmp.left.dataType
  return "string"
}

function operatorsFor(cmp: ConditionComparison): ConditionOperator[] {
  return OPERATORS_BY_DATATYPE[dataTypeOf(cmp)]
}

function patch(p: Partial<ConditionGroup>): void {
  emit("change", { ...props.group, ...p })
}

function updateComparison(index: number, next: ConditionComparison): void {
  patch({ conditions: props.group.conditions.map((c, i) => (i === index ? next : c)) })
}

function onLeftChange(index: number, cmp: ConditionComparison, left: ConditionOperand): void {
  const next: ConditionComparison = { ...cmp, left }
  if (!operatorsFor(next).includes(next.operator)) next.operator = "="
  updateComparison(index, next)
}

function onRightChange(index: number, cmp: ConditionComparison, right: ConditionOperand): void {
  const next: ConditionComparison = { ...cmp, right }
  if (!operatorsFor(next).includes(next.operator)) next.operator = "="
  updateComparison(index, next)
}

function onOperatorChange(index: number, cmp: ConditionComparison, operator: ConditionOperator): void {
  updateComparison(index, { ...cmp, operator })
}

function addRule(): void {
  const blank: ConditionComparison = {
    left: { type: "Literal", value: "", dataType: "string" },
    operator: "=",
    right: { type: "Literal", value: "", dataType: "string" },
  }
  patch({ conditions: [...props.group.conditions, blank] })
}

function removeRule(index: number): void {
  patch({ conditions: props.group.conditions.filter((_, i) => i !== index) })
}

function addCombine(): void {
  patch({ combineCondition: emptyGroup() })
}

function removeCombine(): void {
  patch({ combineCondition: undefined })
}

const conditions = computed(() => props.group.conditions ?? [])
const hasBody = computed(() => conditions.value.length > 0 || !!props.group.combineCondition)
</script>

<template>
  <div class="space-y-1.5">
    <!-- Header row: junction + actions, always on one line (ADR 0010 layout contract). -->
    <div class="flex items-center gap-1.5">
      <Select
        :model-value="group.conditionType"
        :disabled="readonly"
        @update:model-value="(t) => patch({ conditionType: t as 'and' | 'or' })"
      >
        <SelectTrigger size="sm" class="h-7 w-16 border-dashed bg-transparent text-xs shadow-none">
          <SelectValue class="block! min-w-0 truncate" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="and" class="text-xs">且</SelectItem>
          <SelectItem value="or" class="text-xs">或</SelectItem>
        </SelectContent>
      </Select>
      <Button variant="outline" size="sm" class="h-7 text-xs" :disabled="readonly" @click="addRule">
        <Plus :size="12" class="mr-1" />添加规则
      </Button>
      <Button
        v-if="!group.combineCondition"
        variant="outline"
        size="sm"
        class="h-7 text-xs"
        :disabled="readonly"
        @click="addCombine"
      >
        <Plus :size="12" class="mr-1" />组合条件
      </Button>
    </div>

    <!-- Body: vertical rail under the junction's centre + indented content. -->
    <div v-if="hasBody" class="flex gap-2">
      <div class="bg-border ml-[27px] w-px shrink-0 self-stretch" />
      <div class="min-w-0 flex-1 space-y-1.5">
        <!-- Flat comparison rows (conditions[]). -->
        <div v-for="(cmp, index) in conditions" :key="index" class="flex items-start gap-1">
          <div class="min-w-0 flex-1 space-y-1">
            <ConditionOperandEditor
              :operand="cmp.left"
              :designer="designer"
              :readonly="readonly"
              @change="(o) => onLeftChange(index, cmp, o)"
            />
            <div class="flex items-center gap-1">
              <Select
                :model-value="cmp.operator"
                :disabled="readonly"
                @update:model-value="(op) => onOperatorChange(index, cmp, op as ConditionOperator)"
              >
                <SelectTrigger size="sm" class="h-8 w-24 shrink-0 text-xs">
                  <SelectValue class="block! min-w-0 truncate" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="op in operatorsFor(cmp)" :key="op" :value="op" class="text-xs">
                    {{ OPERATOR_LABELS[op] }}
                  </SelectItem>
                </SelectContent>
              </Select>
              <ConditionOperandEditor
                v-if="!isUnary(cmp.operator)"
                class="min-w-0 flex-1"
                :operand="cmp.right"
                :designer="designer"
                :readonly="readonly"
                @change="(o) => onRightChange(index, cmp, o)"
              />
            </div>
          </div>
          <Button
            variant="ghost"
            size="icon"
            class="h-8 w-8 shrink-0"
            :disabled="readonly"
            @click="removeRule(index)"
          >
            <Trash2 :size="13" />
          </Button>
        </div>

        <!-- Nested subgroup (combineCondition) — recursive, indented, dashed. -->
        <div v-if="group.combineCondition" class="border-border rounded-md border border-dashed p-2">
          <div class="mb-1 flex justify-end">
            <Button variant="ghost" size="icon" class="h-6 w-6" :disabled="readonly" @click="removeCombine">
              <Trash2 :size="12" />
            </Button>
          </div>
          <ConditionGroupEditor
            :group="group.combineCondition"
            :designer="designer"
            :readonly="readonly"
            nested
            @change="(g) => patch({ combineCondition: g })"
          />
        </div>
      </div>
    </div>
  </div>
</template>
