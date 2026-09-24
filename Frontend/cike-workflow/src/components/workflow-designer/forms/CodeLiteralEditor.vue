<script setup lang="ts">
import { ref, watch } from "vue"
import MonacoEditor from "../MonacoEditor.vue"

/**
 * Literal-slot editor for code-shaped values (JSON bodies, JS scripts):
 * Monaco with syntax highlighting, edited as a local draft and handed back
 * raw on blur so the caller commits one undo step per edit session (same
 * convention as ExpressionEditor's own Monaco branch). The caller keeps the
 * coercion/commit because only it knows the Input's target type.
 */
const props = withDefaults(
  defineProps<{ value: unknown; language?: string; readonly?: boolean; height?: string }>(),
  { language: "json", readonly: false, height: "120px" },
)
const emit = defineEmits<{ (e: "blur", raw: string): void }>()

// Objects render pretty-printed so long JSON stays readable in Monaco; the
// committed model is parsed back, so formatting is display-only.
function asText(value: unknown): string {
  if (value == null) return ""
  if (typeof value === "object") return JSON.stringify(value, null, 2)
  return String(value)
}

const draft = ref(asText(props.value))
// External model mutations (undo/redo, node switch, commit normalization)
// arrive as a new prop; resync the draft unless it already matches.
watch(
  () => props.value,
  (next) => {
    const text = asText(next)
    if (text !== draft.value) draft.value = text
  },
)
</script>

<template>
  <MonacoEditor
    v-model="draft"
    :language="language"
    :readonly="readonly"
    :height="height"
    @blur="emit('blur', draft)"
  />
</template>
