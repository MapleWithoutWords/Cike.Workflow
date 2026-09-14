<script setup lang="ts">
import PagePlaceholder from '@/components/layout/PagePlaceholder.vue'
</script>

<template>
  <div class="flex flex-col gap-6">
    <header class="flex items-baseline justify-between">
      <h1 class="text-xl font-semibold tracking-tight">工作流定义</h1>
      <p class="text-xs text-muted-foreground">
        单层钻入列表 · 层内搜索与排序
      </p>
    </header>

    <PagePlaceholder
      title="工作流定义列表尚未接入"
      description="定义管理将按 pages/definitions-list.md 规格落地：目录浏览 + 定义列表 + 版本时间线 + 发布/回滚。"
    />
  </div>
</template>
