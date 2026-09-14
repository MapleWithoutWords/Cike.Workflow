<script setup lang="ts">
import PagePlaceholder from '@/components/layout/PagePlaceholder.vue'
</script>

<template>
  <div class="flex flex-col gap-6">
    <header class="flex items-baseline justify-between">
      <h1 class="text-xl font-semibold tracking-tight">工作流实例</h1>
      <p class="text-xs text-muted-foreground">
        实例监控 · 状态三重冗余
      </p>
    </header>

    <PagePlaceholder
      title="工作流实例尚未接入"
      description="后端实例域查询与控制端点均未实现（MASTER §8）。本期为取消/挂起保留位置但不放假按钮。"
    />
  </div>
</template>
