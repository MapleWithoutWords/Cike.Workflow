<script setup lang="ts">
import { computed } from "vue"
import { Input as UiInput } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Plus, Trash2 } from "@lucide/vue"
import { makeEditPropertyCommand } from "@/core/designer/commands"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import type { ExpressionLike } from "@/core/designer/expression"
import ExpressionEditor from "../ExpressionEditor.vue"

const props = defineProps<{ activity: unknown; designer: WorkflowDesignerState }>()

interface CaseRow {
  label: string
  value: string
}

const activity = computed(() => props.activity as { cases?: Array<Record<string, unknown>>; mode?: { expression: ExpressionLike } })

// `mode` is an Input (expression-backed) → edited through ExpressionEditor.
// `cases` is a custom array structure, not an Input → left as-is (spec scope).
const modeExpr = computed<ExpressionLike | null>(() => activity.value.mode?.expression ?? null)

const rows = computed<CaseRow[]>(() =>
  (activity.value.cases ?? []).map((entry) => {
    const value = entry.value as { value?: unknown } | undefined
    return {
      label: String(entry.label ?? ""),
      value: value?.value == null ? "" : typeof value.value === "object" ? JSON.stringify(value.value) : String(value.value),
    }
  }),
)

function commitCases(next: CaseRow[]): void {
  const activityRecord = props.activity as unknown as Record<string, unknown>
  const from = activityRecord["cases"]
  const to = next.map((row) => ({
    label: row.label,
    value: { type: "Literal", value: row.value },
  }))
  props.designer.executeCommand(makeEditPropertyCommand(activityRecord, "cases", from, to))
}

function updateRow(index: number, patch: Partial<CaseRow>): void {
  const next = rows.value.map((row, i) => (i === index ? { ...row, ...patch } : row))
  commitCases(next)
}

function addRow(): void {
  commitCases([...rows.value, { label: "", value: "" }])
}

function removeRow(index: number): void {
  commitCases(rows.value.filter((_, i) => i !== index))
}
</script>

<template>
  <div class="space-y-3">
    <ExpressionEditor v-if="modeExpr" :expression="modeExpr" :designer="designer" label="匹配模式" :literal-default="0">
      <template #default="{ value, commit }">
        <Select :model-value="String(value ?? 0)" @update:model-value="(v) => commit(Number(String(v)))">
          <SelectTrigger class="h-8 text-xs"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="0">MatchFirst（首个匹配）</SelectItem>
            <SelectItem value="1">MatchAny（任一匹配）</SelectItem>
          </SelectContent>
        </Select>
      </template>
    </ExpressionEditor>

    <div class="space-y-1">
      <Label class="text-xs">分支（Case）</Label>
      <div v-for="(row, index) in rows" :key="index" class="flex items-center gap-1.5">
        <UiInput
          class="h-8 w-24 text-xs"
          :model-value="row.label"
          placeholder="标签（出端口）"
          @change="(event: Event) => updateRow(index, { label: (event.target as HTMLInputElement).value })"
        />
        <UiInput
          class="h-8 flex-1 text-xs"
          :model-value="row.value"
          placeholder="匹配值"
          @change="(event: Event) => updateRow(index, { value: (event.target as HTMLInputElement).value })"
        />
        <Button variant="ghost" size="icon" class="h-8 w-8 shrink-0" @click="removeRow(index)">
          <Trash2 :size="13" />
        </Button>
      </div>
      <Button variant="outline" size="sm" class="w-full" @click="addRow">
        <Plus :size="13" class="mr-1" />添加分支
      </Button>
      <div class="text-[10px] text-muted-foreground">每个分支标签都会成为该节点的一个出端口</div>
    </div>
  </div>
</template>
