<script setup lang="ts">
import PagePlaceholder from '@/components/layout/PagePlaceholder.vue'
</script>

<template>
  <div class="flex flex-col gap-6">
    <header class="flex items-baseline justify-between">
      <h1 class="text-xl font-semibold tracking-tight">首页</h1>
      <p class="text-xs text-muted-foreground">
        全局概览 · 跨空间视角
      </p>
    </header>

    <PagePlaceholder
      title="首页聚合数据尚未接入"
      description="KPI（工作空间 / 定义 / 运行中 / 今日故障）、最近工作空间、故障聚焦、最近执行等区块将在后端聚合能力就绪后填充。"
    >
      <p class="text-sm text-muted-foreground">
        当前为 P0 应用外壳阶段。后续阶段会按
        <code class="rounded bg-muted px-1 py-0.5 font-mono text-xs">pages/home.md</code>
        的规格逐步落地。
      </p>
    </PagePlaceholder>
  </div>
</template>
