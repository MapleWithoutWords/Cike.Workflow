<script setup lang="ts">
import PagePlaceholder from '@/components/layout/PagePlaceholder.vue'
</script>

<template>
  <div class="flex flex-col gap-6">
    <header class="flex items-baseline justify-between">
      <h1 class="text-xl font-semibold tracking-tight">工作空间</h1>
      <p class="text-xs text-muted-foreground">
        数据域根 · 其下挂目录 / 定义 / 实例
      </p>
    </header>

    <PagePlaceholder
      title="工作空间列表尚未接入"
      description="列表将按 pages/workspace.md 规格呈现：工具栏（搜索 + 新建）+ 卡片网格；不显示定义/实例统计（后端无跨层聚合能力）。"
    />
  </div>
</template>
