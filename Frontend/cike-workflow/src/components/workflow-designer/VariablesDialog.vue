<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { Plus, Trash2 } from "@lucide/vue"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input as UiInput } from "@/components/ui/input"
import { Switch } from "@/components/ui/switch"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import { variableNameIssues } from "@/core/designer/form"

const props = defineProps<{ designer: WorkflowDesignerState; open: boolean }>()

const emit = defineEmits<{ "update:open": [open: boolean] }>()

interface VariableRow {
  id: string
  name: string
  typeName: string
  isArray: boolean
}

const rows = ref<VariableRow[]>([])

watch(
  () => props.open,
  (open) => {
    if (!open) return
    rows.value = props.designer.variables.value.map((variable) => ({
      id: variable.id ?? crypto.randomUUID(),
      name: variable.name ?? "",
      typeName: variable.typeName ?? "String",
      isArray: variable.isArray ?? false,
    }))
  },
  { immediate: true },
)

const issues = computed(() => variableNameIssues(rows.value))

function addRow(): void {
  rows.value.push({ id: crypto.randomUUID(), name: "", typeName: "String", isArray: false })
}

function removeRow(id: string): void {
  rows.value = rows.value.filter((row) => row.id !== id)
}

function confirm(): void {
  props.designer.setVariables(
    rows.value.map((row) => ({ id: row.id, name: row.name, typeName: row.typeName, isArray: row.isArray })),
  )
  emit("update:open", false)
}
</script>

<template>
  <Dialog :open="open" @update:open="(value: boolean) => emit('update:open', value)">
    <DialogContent class="max-w-xl">
      <DialogHeader>
        <DialogTitle>工作流变量</DialogTitle>
      </DialogHeader>
      <div class="max-h-80 space-y-2 overflow-y-auto">
        <div v-if="rows.length === 0" class="py-6 text-center text-xs text-muted-foreground">
          暂无变量，点击下方按钮添加
        </div>
        <div v-for="row in rows" :key="row.id" class="flex items-center gap-2">
          <UiInput v-model="row.name" class="h-8 flex-1 text-xs" placeholder="变量名" />
          <UiInput v-model="row.typeName" class="h-8 w-28 text-xs" placeholder="类型" />
          <label class="flex shrink-0 items-center gap-1 text-xs text-muted-foreground">
            数组
            <Switch :model-value="row.isArray" @update:model-value="(checked: boolean) => (row.isArray = checked)" />
          </label>
          <Button variant="ghost" size="icon" class="h-8 w-8 shrink-0" title="删除变量" @click="removeRow(row.id)">
            <Trash2 :size="14" />
          </Button>
        </div>
      </div>
      <div class="text-xs text-destructive">
        <div v-for="issue in issues" :key="issue">{{ issue }}</div>
      </div>
      <DialogFooter class="justify-between">
        <Button variant="outline" size="sm" @click="addRow">
          <Plus :size="14" class="mr-1" />添加变量
        </Button>
        <Button size="sm" :disabled="issues.length > 0" @click="confirm">确定</Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
