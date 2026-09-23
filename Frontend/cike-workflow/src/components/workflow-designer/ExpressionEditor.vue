<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { Input as UiInput } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import { makeEditPropertyCommand } from "@/core/designer/commands"
import { LITERAL_TYPE, isLiteral, makeSwitchExpressionTypeCommand, type ExpressionLike } from "@/core/designer/expression"
import MonacoEditor from "./MonacoEditor.vue"

/**
 * Unified editor for one Input's Expression (ADR 0004). It owns the type
 * selector (backend ExpressionDescriptors-driven), the four non-Literal editors,
 * and every model write through the designer command stack. The caller supplies
 * only the Literal value editor via the default slot, because only the node
 * knows the Input's target type.
 */
const props = defineProps<{
  expression: ExpressionLike
  designer: WorkflowDesignerState
  label?: string
  description?: string | null
  /** Concrete zero/default value used when switching *to* Literal (ADR 0005). */
  literalDefault?: unknown
  readonly?: boolean
}>()

const isReadonly = computed(() => props.readonly ?? props.designer.readonly.value)

// Reading `revision` makes the editor re-render on any model mutation (including
// undo/redo and type switches), since the domain model lives in shallowRefs and
// nested plain objects are not reactive on their own.
const currentType = computed(() => {
  void props.designer.revision.value
  return props.expression.type || LITERAL_TYPE
})

const currentValue = computed(() => {
  void props.designer.revision.value
  return props.expression.value
})

// Backend-registered types; a Literal-only fallback covers the pre-fetch moment.
const typeOptions = computed(() => {
  const list = props.designer.expressionDescriptors.value
  if (list.length === 0) return [{ type: LITERAL_TYPE, displayName: "字面量" }]
  return list
    .filter((d): d is { type: string; displayName?: string } => !!d.type)
    .map((d) => ({ type: d.type as string, displayName: d.displayName ?? d.type }))
})

const isMonacoType = computed(() => currentType.value === "JavaScript" || currentType.value === "Liquid")
const isNameType = computed(() => currentType.value === "Variable" || currentType.value === "WorkflowInput")

function languageFor(type: string): string {
  if (type === "JavaScript") return "javascript"
  if (type === "Liquid") return "liquid"
  return "plaintext"
}

const variableNames = computed(() =>
  props.designer.variables.value.map((v) => v.name ?? "").filter((n) => n.length > 0),
)
const inputNames = computed(() =>
  props.designer.inputs.value.map((i) => i.name ?? "").filter((n) => n.length > 0),
)
const nameOptions = computed(() => (currentType.value === "Variable" ? variableNames.value : inputNames.value))
const nameValue = computed(() => (typeof currentValue.value === "string" ? currentValue.value : ""))

// Local draft for text editors: commit on blur so undo history is one step per
// edit session, not one per keystroke.
const draft = ref("")
watch(
  [currentValue, currentType],
  () => {
    draft.value = typeof currentValue.value === "string" ? currentValue.value : ""
  },
  { immediate: true },
)

function commitValue(to: unknown): void {
  if (isReadonly.value) return
  const from = props.expression.value
  if (from === to) return
  props.designer.executeCommand(
    makeEditPropertyCommand(props.expression as unknown as Record<string, unknown>, "value", from, to),
  )
}

function commitDraft(): void {
  commitValue(draft.value)
}

function onTypeChange(type: string): void {
  if (type === currentType.value) return
  props.designer.executeCommand(
    makeSwitchExpressionTypeCommand(props.expression, type, props.literalDefault),
  )
}
</script>

<template>
  <div class="space-y-1">
    <div class="flex items-center gap-2">
      <Label v-if="label" class="text-xs" :title="description ?? undefined">{{ label }}</Label>
      <div class="ml-auto">
        <Select
          :model-value="currentType"
          :disabled="isReadonly"
          @update:model-value="(t) => onTypeChange(String(t))"
        >
          <SelectTrigger class="h-6 w-32 text-xs">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="opt in typeOptions" :key="opt.type" :value="opt.type">
              {{ opt.displayName }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>

    <!-- Literal: the caller-provided value editor (only the node knows T). -->
    <slot v-if="isLiteral(currentType)" :value="currentValue" :commit="commitValue" :readonly="isReadonly" />

    <!-- JavaScript / Liquid: Monaco, committed on blur. -->
    <MonacoEditor
      v-else-if="isMonacoType"
      v-model="draft"
      :language="languageFor(currentType)"
      :readonly="isReadonly"
      @blur="commitDraft"
    />

    <!-- Variable / WorkflowInput: reference a definition by name. -->
    <Select
      v-else-if="isNameType"
      :model-value="nameValue"
      :disabled="isReadonly"
      @update:model-value="(n) => commitValue(String(n))"
    >
      <SelectTrigger class="h-8 text-xs">
        <SelectValue :placeholder="nameOptions.length ? '选择…' : '（暂无可选项）'" />
      </SelectTrigger>
      <SelectContent>
        <SelectItem v-for="name in nameOptions" :key="name" :value="name">{{ name }}</SelectItem>
      </SelectContent>
    </Select>

    <!-- Any other string-valued type: plain text fallback. -->
    <UiInput
      v-else
      class="font-mono text-xs"
      :model-value="draft"
      :disabled="isReadonly"
      @input="(e: Event) => (draft = (e.target as HTMLInputElement).value)"
      @blur="commitDraft"
    />
  </div>
</template>
