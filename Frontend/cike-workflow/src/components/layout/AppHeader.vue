<script setup lang="ts">
import { useMediaQuery } from '@vueuse/core'
import { computed } from 'vue'
import { Separator } from '@/components/ui/separator'
import { SidebarTrigger } from '@/components/ui/sidebar'
import AppBreadcrumb from './AppBreadcrumb.vue'
import ThemeToggle from './ThemeToggle.vue'

const props = defineProps<{
  /** 空间上下文内才有 L2 可折叠 */
  hasWorkspaceNav: boolean
}>()

const isMobile = useMediaQuery('(max-width: 1023px)')

/** 只有存在可折叠侧栏时才显示触发器 */
const showTrigger = computed(() => isMobile.value || props.hasWorkspaceNav)
</script>

<template>
  <header
    class="sticky top-0 z-20 flex h-14 shrink-0 items-center gap-2 border-b bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60"
  >
    <template v-if="showTrigger">
      <SidebarTrigger class="-ml-1" />
      <Separator orientation="vertical" class="mr-2 !h-4" />
    </template>

    <AppBreadcrumb class="min-w-0 flex-1" />

    <div class="flex items-center gap-1">
      <!-- ⌘K 命令面板是 P5，本期不放假按钮 -->
      <ThemeToggle />
    </div>
  </header>
</template>
