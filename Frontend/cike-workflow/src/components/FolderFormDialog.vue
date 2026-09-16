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
import { Label } from "@/components/ui/label"
import { postApiV1Folders, putApiV1Folders } from "@/api"

const props = defineProps<{
  open: boolean
  folder?: { id: string; name: string } | null
  parentId: string
  workspaceId: string
}>()

const emit = defineEmits<{
  (e: "update:open", value: boolean): void
  (e: "saved"): void
}>()

const isEdit = () => !!props.folder?.id

const name = ref("")
const submitting = ref(false)
const errorMessage = ref("")

watch(
  () => props.open,
  (val) => {
    if (val) {
      errorMessage.value = ""
      name.value = props.folder?.name ?? ""
    }
  },
)

function close() {
  emit("update:open", false)
}

async function handleSubmit() {
  if (!name.value.trim()) return

  submitting.value = true
  errorMessage.value = ""
  try {
    const { error } = isEdit()
      ? await putApiV1Folders({
          query: { folderId: props.folder!.id },
          body: { name: name.value },
        })
      : await postApiV1Folders({
          body: {
            workspaceId: props.workspaceId,
            name: name.value,
            parentId: props.parentId,
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
        <DialogTitle>{{ folder?.id ? '重命名目录' : '新建目录' }}</DialogTitle>
        <DialogDescription>
          {{ folder?.id ? '修改目录名称' : '在当前目录下创建新的目录' }}
        </DialogDescription>
      </DialogHeader>

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div class="space-y-2">
          <Label for="folder-name">目录名称 <span class="text-destructive">*</span></Label>
          <Input
            id="folder-name"
            v-model="name"
            placeholder="如 报销流程"
            required
            autofocus
          />
        </div>

        <p v-if="errorMessage" class="text-sm text-destructive">
          {{ errorMessage }}
        </p>

        <DialogFooter>
          <Button type="button" variant="outline" @click="close">
            取消
          </Button>
          <Button type="submit" :disabled="submitting || !name.trim()">
            {{ submitting ? '提交中...' : (folder?.id ? '保存' : '创建') }}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>
