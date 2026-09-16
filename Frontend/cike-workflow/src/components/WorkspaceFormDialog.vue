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
import { postApiV1Workspaces, putApiV1Workspaces } from "@/api"
import type { WorkspaceItemDto } from "@/api"

const props = defineProps<{
  open: boolean
  workspace?: WorkspaceItemDto | null
}>()

const emit = defineEmits<{
  (e: "update:open", value: boolean): void
  (e: "saved"): void
}>()

const isEdit = () => !!props.workspace?.id

const form = ref({
  code: "",
  name: "",
  description: "",
})

const submitting = ref(false)
const errorMessage = ref("")

// 打开对话框时初始化表单
watch(
  () => props.open,
  (val) => {
    if (val) {
      errorMessage.value = ""
      form.value = {
        code: props.workspace?.code ?? "",
        name: props.workspace?.name ?? "",
        description: props.workspace?.description ?? "",
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
      ? await putApiV1Workspaces({
          query: { workspaceId: props.workspace!.id! },
          body: { name: form.value.name, description: form.value.description },
        })
      : await postApiV1Workspaces({
          body: {
            code: form.value.code,
            name: form.value.name,
            description: form.value.description,
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
    <DialogContent class="sm:max-w-md">
      <DialogHeader>
        <DialogTitle>{{ workspace?.id ? '编辑空间' : '新建空间' }}</DialogTitle>
        <DialogDescription>
          {{ workspace?.id ? '修改工作空间的名称和描述' : '创建一个新的工作空间' }}
        </DialogDescription>
      </DialogHeader>

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div class="space-y-2">
          <Label for="ws-code">编码</Label>
          <Input
            id="ws-code"
            v-model="form.code"
            placeholder="如 FIN、CRM"
            :disabled="!!workspace?.id"
          />
          <p v-if="workspace?.id" class="text-xs text-muted-foreground">
            编码创建后不可修改
          </p>
        </div>

        <div class="space-y-2">
          <Label for="ws-name">名称 <span class="text-destructive">*</span></Label>
          <Input
            id="ws-name"
            v-model="form.name"
            placeholder="如 财务系统"
            required
          />
        </div>

        <div class="space-y-2">
          <Label for="ws-desc">描述</Label>
          <Textarea
            id="ws-desc"
            v-model="form.description"
            placeholder="简要描述该空间的用途"
            rows="3"
          />
        </div>

        <p v-if="errorMessage" class="text-sm text-destructive">
          {{ errorMessage }}
        </p>

        <DialogFooter>
          <Button type="button" variant="outline" @click="close">
            取消
          </Button>
          <Button type="submit" :disabled="submitting || !form.name.trim()">
            {{ submitting ? '提交中...' : (workspace?.id ? '保存' : '创建') }}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>
