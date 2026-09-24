<script setup lang="ts">
import { computed } from "vue"
import { Input as UiInput } from "@/components/ui/input"
import { Switch } from "@/components/ui/switch"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { ConditionDataType, ConditionOperand } from "@/core/designer/conditionCompile"

/**
 * One operand (left or right of a comparison). Controlled: emits a new operand
 * on every edit; owns no command logic (ADR 0010).
 *   Literal    → dataType selector + a widget per dataType
 *   Javascript → a code input (e.g. getVariable('x'))
 */
const props = defineProps<{ operand: ConditionOperand; readonly?: boolean }>()
const emit = defineEmits<{ change: [ConditionOperand] }>()

const isLiteral = computed(() => props.operand.type !== "Javascript")
const dataType = computed<ConditionDataType>(() => props.operand.dataType ?? "string")

function defaultFor(dt: ConditionDataType): unknown {
  return dt === "number" ? 0 : dt === "boolean" ? false : ""
}

function setType(type: "Literal" | "Javascript"): void {
  if (type === props.operand.type) return
  emit(
    "change",
    type === "Literal"
      ? { type, value: defaultFor(dataType.value), dataType: dataType.value }
      : { type, value: typeof props.operand.value === "string" ? props.operand.value : "" },
  )
}

function setDataType(dt: ConditionDataType): void {
  emit("change", { ...props.operand, dataType: dt, value: defaultFor(dt) })
}

function setValue(value: unknown): void {
  emit("change", { ...props.operand, value })
}
</script>

<template>
  <div class="flex min-w-0 items-center gap-1">
    <Select :model-value="operand.type" :disabled="readonly" @update:model-value="(t) => setType(t as 'Literal' | 'Javascript')">
      <SelectTrigger size="sm" class="h-8 w-20 shrink-0 text-xs">
        <SelectValue class="block! min-w-0 truncate" />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value="Literal" class="text-xs">字面量</SelectItem>
        <SelectItem value="Javascript" class="text-xs">脚本</SelectItem>
      </SelectContent>
    </Select>

    <template v-if="isLiteral">
      <Select :model-value="dataType" :disabled="readonly" @update:model-value="(d) => setDataType(d as ConditionDataType)">
        <SelectTrigger size="sm" class="h-8 w-24 shrink-0 text-xs">
          <SelectValue class="block! min-w-0 truncate" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="string" class="text-xs">字符串</SelectItem>
          <SelectItem value="number" class="text-xs">数字</SelectItem>
          <SelectItem value="boolean" class="text-xs">布尔</SelectItem>
          <SelectItem value="datetime" class="text-xs">日期时间</SelectItem>
        </SelectContent>
      </Select>

      <div class="min-w-0 flex-1">
        <div v-if="dataType === 'boolean'" class="flex h-8 items-center gap-2">
          <Switch :model-value="operand.value === true" :disabled="readonly" @update:model-value="(v: boolean) => setValue(v)" />
          <span class="text-xs text-muted-foreground">{{ operand.value === true ? "True" : "False" }}</span>
        </div>
        <UiInput
          v-else-if="dataType === 'number'"
          class="h-8 text-xs"
          type="number"
          :model-value="operand.value == null ? '' : String(operand.value)"
          :disabled="readonly"
          @change="(e: Event) => setValue(Number((e.target as HTMLInputElement).value))"
        />
        <UiInput
          v-else-if="dataType === 'datetime'"
          class="h-8 text-xs"
          type="datetime-local"
          :model-value="operand.value == null ? '' : String(operand.value)"
          :disabled="readonly"
          @change="(e: Event) => setValue((e.target as HTMLInputElement).value)"
        />
        <UiInput
          v-else
          class="h-8 text-xs"
          :model-value="operand.value == null ? '' : String(operand.value)"
          :disabled="readonly"
          @change="(e: Event) => setValue((e.target as HTMLInputElement).value)"
        />
      </div>
    </template>

    <UiInput
      v-else
      class="h-8 min-w-0 flex-1 font-mono text-xs"
      placeholder="getVariable('x')"
      :model-value="typeof operand.value === 'string' ? operand.value : ''"
      :disabled="readonly"
      @change="(e: Event) => setValue((e.target as HTMLInputElement).value)"
    />
  </div>
</template>
