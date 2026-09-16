<script setup lang="ts">
import { ref, watch } from "vue"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Label } from "@/components/ui/label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Switch } from "@/components/ui/switch"
import {
  postApiV1WorkflowDefinitions,
  putApiV1WorkflowDefinitionsById,
} from "@/api"
import type { WorkflowDefinitionFolderItemDto, WorkflowDefinitionItemDto, WorkflowDefinitionType } from "@/api"

const props = defineProps<{
  open: boolean
  definition?: WorkflowDefinitionFolderItemDto | null
  folderId: string
  workspaceId: string
}>()

const emit = defineEmits<{
  (e: "update:open", value: boolean): void
  (e: "saved"): void
}>()

const isEdit = () => !!props.definition?.id

// 类型映射：1=Workflow, 2=AgentWorkflow, 3=Approval
const typeOptions = [
  { value: 1, label: "Workflow" },
  { value: 2, label: "AgentWorkflow" },
  { value: 3, label: "Approval" },
] as const

const form = ref({
  definitionId: "",
  name: "",
  description: "",
  type: 1 as WorkflowDefinitionType,
  usableAsActivity: false,
})

const submitting = ref(false)
const errorMessage = ref("")

watch(
  () => props.open,
  (val) => {
    if (val) {
      errorMessage.value = ""
      if (props.definition?.data) {
        const data = props.definition.data as WorkflowDefinitionItemDto
        form.value = {
          definitionId: data.definitionId ?? "",
          name: data.name ?? "",
          description: data.description ?? "",
          type: data.type ?? 1,
          usableAsActivity: data.usableAsActivity ?? false,
        }
      } else {
        form.value = { definitionId: "", name: "", description: "", type: 1, usableAsActivity: false }
      }
    }
  },
)

function close() {
  emit("update:open", false)
}

async function handleSubmit() {
  if (!form.value.name.trim()) return

  submitting.value = true
  errorMessage.value = ""
  try {
    const { error } = isEdit()
      ? await putApiV1WorkflowDefinitionsById({
          path: { id: props.definition!.id! },
          body: {
            name: form.value.name,
            description: form.value.description,
            type: form.value.type,
            usableAsActivity: form.value.usableAsActivity,
          },
        })
      : await postApiV1WorkflowDefinitions({
          body: {
            workspaceId: props.workspaceId,
            folderId: props.folderId,
            definitionId: form.value.definitionId.trim() || null,
            name: form.value.name,
            description: form.value.description,
            type: form.value.type,
            usableAsActivity: form.value.usableAsActivity,
          },
        })
    if (error) {
      errorMessage.value = "保存失败，请稍后重试"
      return
    }
    close()
    emit("saved")
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <Dialog :open="open" @update:open="emit('update:open', $event)">
    <DialogContent class="sm:max-w-lg">
      <DialogHeader>
        <DialogTitle>{{ definition?.id ? '编辑定义' : '新建定义' }}</DialogTitle>
        <DialogDescription>
          {{ definition?.id ? '修改工作流定义的基本信息' : '在当前目录下创建新的工作流定义' }}
        </DialogDescription>
      </DialogHeader>

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div class="space-y-2">
          <Label for="def-id">定义 ID</Label>
          <Input
            id="def-id"
            v-model="form.definitionId"
            :disabled="!!definition?.id"
            placeholder="留空则由系统自动生成"
          />
          <p v-if="!definition?.id" class="text-xs text-muted-foreground">
            可选。自定义工作流定义的唯一标识，留空则由后端自动生成。
          </p>
        </div>

        <div class="space-y-2">
          <Label for="def-name">名称 <span class="text-destructive">*</span></Label>
          <Input
            id="def-name"
            v-model="form.name"
            placeholder="如 月度报销审批"
            required
          />
        </div>

        <div class="space-y-2">
          <Label for="def-desc">描述</Label>
          <Textarea
            id="def-desc"
            v-model="form.description"
            placeholder="简要描述该定义的用途"
            rows="3"
          />
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <Label>类型</Label>
            <Select
              :model-value="String(form.type)"
              @update:model-value="form.type = Number($event) as WorkflowDefinitionType"
            >
              <SelectTrigger class="w-full">
                <SelectValue placeholder="选择类型" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="opt in typeOptions" :key="opt.value" :value="String(opt.value)">
                  {{ opt.label }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div class="space-y-2">
            <Label>可作为活动使用</Label>
            <div class="flex h-9 items-center gap-2">
              <Switch v-model="form.usableAsActivity" />
              <span class="text-sm text-muted-foreground">
                {{ form.usableAsActivity ? '是' : '否' }}
              </span>
            </div>
          </div>
        </div>

        <p v-if="errorMessage" class="text-sm text-destructive">
          {{ errorMessage }}
        </p>

        <DialogFooter>
          <Button type="button" variant="outline" @click="close">
            取消
          </Button>
          <Button type="submit" :disabled="submitting || !form.name.trim()">
            {{ submitting ? '提交中...' : (definition?.id ? '保存' : '创建') }}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>
