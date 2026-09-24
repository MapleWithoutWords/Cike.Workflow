<script setup lang="ts">
import { computed } from "vue"
import { Button } from "@/components/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Plus, Trash2 } from "@lucide/vue"
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
 * One condition group (recursive). The layout mirrors the data 1:1 (ADR 0010):
 * flat comparison rows = conditions[], the junction 且/或 button = conditionType,
 * the indented dashed box = combineCondition (renders itself recursively).
 * Controlled: emits a new group; owns no command logic.
 */
defineOptions({ name: "ConditionGroupEditor" })

const props = defineProps<{ group: ConditionGroup; readonly?: boolean; nested?: boolean }>()
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

/** The comparison's dataType governs the operator set (right literal wins). */
function dataTypeOf(cmp: ConditionComparison): ConditionDataType {
  if (cmp.right?.type !== "Javascript" && cmp.right?.dataType) return cmp.right.dataType
  if (cmp.left?.type !== "Javascript" && cmp.left?.dataType) return cmp.left.dataType
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
    left: { type: "Javascript", value: "" },
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
</script>

<template>
  <div class="flex gap-2">
    <!-- Junction rail: the conditionType toggle joining this row-set and the subgroup. -->
    <div class="flex shrink-0 flex-col items-center gap-1">
      <Select
        :model-value="group.conditionType"
        :disabled="readonly"
        @update:model-value="(t) => patch({ conditionType: t as 'and' | 'or' })"
      >
        <SelectTrigger size="sm" class="h-7 w-16 text-xs">
          <SelectValue class="block! min-w-0 truncate" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="and" class="text-xs">且</SelectItem>
          <SelectItem value="or" class="text-xs">或</SelectItem>
        </SelectContent>
      </Select>
      <div class="bg-border w-px flex-1" />
    </div>

    <div class="min-w-0 flex-1 space-y-1.5">
      <!-- Flat comparison rows (conditions[]). -->
      <div v-for="(cmp, index) in conditions" :key="index" class="flex items-start gap-1">
        <div class="min-w-0 flex-1 space-y-1">
          <ConditionOperandEditor
            :operand="cmp.left"
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
          :readonly="readonly"
          nested
          @change="(g) => patch({ combineCondition: g })"
        />
      </div>

      <!-- Actions. -->
      <div class="flex gap-1.5 pt-0.5">
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
    </div>
  </div>
</template>
