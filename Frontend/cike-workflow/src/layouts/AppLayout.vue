<script setup lang="ts">
import { useMediaQuery } from '@vueuse/core'
import { computed, ref, watch } from 'vue'
import { RouterView } from 'vue-router'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import AppSidebarRail from '@/components/layout/AppSidebarRail.vue'
import { SidebarInset, SidebarProvider } from '@/components/ui/sidebar'
import { useWorkspaceContext } from '@/composables/useWorkspaceContext'

const { inWorkspace } = useWorkspaceContext()
const isDesktopWide = useMediaQuery('(min-width: 1280px)')

/**
 * L2 展开策略（MASTER §7 断点）：
 * - ≥1280：展开
 * - 1024–1279：收成图标栏
 * - <1024：走 Sheet 抽屉（由 Sidebar 组件内部按 isMobile 处理）
 *
 * 必须是可写 ref：SidebarTrigger / SidebarRail 会回写该状态。
 */
const sidebarOpen = ref(isDesktopWide.value)
watch(isDesktopWide, (wide) => {
    sidebarOpen.value = wide
})

// 是否处于空间上下文，决定 L2 内容（AppSidebar 自行判断是否渲染）
const hasWorkspaceNav = computed(() => inWorkspace.value)
</script>

<template>
    <!-- SidebarProvider 同时提供 sidebar context 与 TooltipProvider，必须包住整棵外壳 -->
    <SidebarProvider v-model:open="sidebarOpen">
        <a href="#main-content"
            class="sr-only focus:not-sr-only focus:fixed focus:left-2 focus:top-2 focus:z-50 focus:rounded-md focus:bg-primary focus:px-3 focus:py-1.5 focus:text-sm focus:font-medium focus:text-primary-foreground focus:outline-none">
            跳到主内容
        </a>

        <AppSidebarRail />
        <AppSidebar />

        <SidebarInset id="main-content" tabindex="-1" class="outline-none">
            <AppHeader :has-workspace-nav="hasWorkspaceNav" />
            <div class="flex flex-1 flex-col gap-4 p-4 md:gap-6 md:p-6">
                <RouterView v-slot="{ Component, route: currentRoute }">
                    <Transition name="fade" mode="out-in">
                        <component :is="Component" :key="currentRoute.fullPath" />
                    </Transition>
                </RouterView>
            </div>
        </SidebarInset>
    </SidebarProvider>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
    transition: opacity 200ms ease;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}

@media (prefers-reduced-motion: reduce) {

    .fade-enter-active,
    .fade-leave-active {
        transition-duration: 0ms;
    }
}
</style>
