<script setup lang="ts">
import { useWorkspaceContext } from '@/composables/useWorkspaceContext'
import PagePlaceholder from '@/components/layout/PagePlaceholder.vue'

const { displayName } = useWorkspaceContext()
</script>

<template>
  <div class="flex flex-col gap-6">
    <header class="flex items-baseline justify-between">
      <h1 class="text-xl font-semibold tracking-tight">空间详情</h1>
      <p class="text-xs text-muted-foreground">
        当前空间：<span class="font-mono">{{ displayName }}</span>
      </p>
    </header>

    <PagePlaceholder
      title="空间详情尚未接入"
      description="概览 + 元数据 + 根目录预览 + 快捷入口 + 危险区（重命名 / 描述 / 删除）将按 pages/workspace.md §B 落地。"
    />
  </div>
</template>
