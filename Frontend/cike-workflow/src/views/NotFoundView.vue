<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { Button } from '@/components/ui/button'
import { ArrowLeft } from '@lucide/vue'
import PagePlaceholder from '@/components/layout/PagePlaceholder.vue'
</script>

<template>
  <div class="flex min-h-[60vh] flex-col items-center justify-center gap-6 p-6">
    <PagePlaceholder
      title="页面不存在"
      description="你访问的地址无法匹配任何路由。"
    />
    <Button :as="RouterLink" :to="{ name: 'home' }" variant="outline" size="sm">
      <ArrowLeft class="mr-2 size-4" />
      返回首页
    </Button>
  </div>
</template>
