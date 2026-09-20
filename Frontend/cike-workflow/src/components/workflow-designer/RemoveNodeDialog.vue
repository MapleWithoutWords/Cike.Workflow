<script setup lang="ts">
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog"
import type { DesignerCommand } from "@/core/designer/commands"

defineProps<{
  open: boolean
  pendingCommand: DesignerCommand | null
}>()

const emit = defineEmits<{
  "update:open": [open: boolean]
  confirm: []
}>()
</script>

<template>
  <AlertDialog :open="open" @update:open="(value: boolean) => emit('update:open', value)">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>删除节点</AlertDialogTitle>
        <AlertDialogDescription>
          <template v-if="(pendingCommand?.removedConnectionCount ?? 0) > 0">
            将同时移除该节点关联的 {{ pendingCommand?.removedConnectionCount }} 条连线。
          </template>
          <template v-else>确认删除选中的节点？</template>
        </AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <AlertDialogCancel>取消</AlertDialogCancel>
        <AlertDialogAction @click="emit('confirm')">删除</AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
</template>
