<script setup lang="ts">
import { ref, watch } from "vue"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { compileGroup, type ConditionGroup, type ConditionSpec } from "@/core/designer/conditionCompile"
import { emptyGroup } from "@/core/designer/conditionModel"
import ConditionGroupEditor from "./ConditionGroupEditor.vue"
import MonacoEditor from "./MonacoEditor.vue"

/**
 * Top-level editor for one condition (If's condition / one Switch case).
 * The stored discriminator `type` decides the editor in a single read (ADR 0010):
 *   custom    → the visual builder (ConditionGroupEditor)
 *   Literal   → a true/false switch        (escape hatch)
 *   Javascript/Liquid → Monaco             (escape hatch)
 * Controlled: emits a new ConditionSpec; the owning form writes it to
 * customProperties + the compiled expression as one undoable command.
 */
const props = defineProps<{ spec: ConditionSpec; readonly?: boolean; label?: string }>()
const emit = defineEmits<{ change: [ConditionSpec] }>()

const TYPE_LABELS: Record<ConditionSpec["type"], string> = {
  custom: "可视化",
  Literal: "字面量",
  Javascript: "JavaScript",
  Liquid: "Liquid",
}

function languageFor(type: ConditionSpec["type"]): string {
  return type === "Liquid" ? "liquid" : "javascript"
}

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

function onLiteralChange(value: boolean): void {
  emit("change", { type: "Literal", value })
}

// Monaco draft: commit on blur so one edit session = one undo step.
const draft = ref("")
watch(
  () => [props.spec.type, props.spec.value],
  () => {
    draft.value = typeof props.spec.value === "string" ? props.spec.value : ""
  },
  { immediate: true },
)

function commitDraft(): void {
  if (props.spec.type !== "Javascript" && props.spec.type !== "Liquid") return
  if (draft.value === props.spec.value) return
  emit("change", { type: props.spec.type, value: draft.value })
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
      :readonly="readonly"
      @change="onGroupChange"
    />

    <div v-else-if="spec.type === 'Literal'" class="flex h-8 items-center gap-2">
      <Switch :model-value="spec.value === true" :disabled="readonly" @update:model-value="(v: boolean) => onLiteralChange(v)" />
      <span class="text-xs text-muted-foreground">{{ spec.value === true ? "True" : "False" }}</span>
    </div>

    <MonacoEditor
      v-else
      v-model="draft"
      :language="languageFor(spec.type)"
      :readonly="readonly"
      height="96px"
      @blur="commitDraft"
    />
  </div>
</template>
