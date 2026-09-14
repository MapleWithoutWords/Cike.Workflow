<script setup lang="ts">
import { useRoute, RouterLink } from 'vue-router'
import { Button } from '@/components/ui/button'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import { L1_NAV, isL1Active } from './navigation'

const route = useRoute()
</script>

<template>
    <!-- L1 常驻窄栏。<1024 时并入 L2 抽屉（MASTER §7），故此处 lg 才显示 -->
    <aside class="hidden w-14 shrink-0 flex-col items-center gap-1 border-r bg-sidebar py-2 lg:flex" aria-label="全局导航">
        <RouterLink :to="{ name: 'home' }"
            class="mb-2 flex h-9 w-9 items-center justify-center rounded-md bg-primary text-primary-foreground text-sm font-bold"
            aria-label="Cike.Workflow 首页">
            CW
        </RouterLink>

        <nav class="flex flex-1 flex-col items-center gap-1">
            <Tooltip v-for="item in L1_NAV" :key="item.key">
                <TooltipTrigger as-child>
                    <Button :as="RouterLink" :to="item.to" variant="ghost" size="icon"
                        :aria-current="isL1Active(item, route.name as string | undefined, route.path) ? 'page' : undefined"
                        :class="[
                            'relative h-9 w-9',
                            isL1Active(item, route.name as string | undefined, route.path)
                                ? 'bg-sidebar-accent text-sidebar-accent-foreground font-medium'
                                : 'hover:bg-sidebar-accent hover:text-sidebar-accent-foreground',
                        ]">
                        <component :is="item.icon" class="size-4" />
                        <span class="sr-only">{{ item.label }}</span>
                        <span v-if="isL1Active(item, route.name as string | undefined, route.path)"
                            class="absolute left-0 top-1/2 h-5 w-0.5 -translate-y-1/2 rounded-r bg-primary"
                            aria-hidden="true" />
                    </Button>
                </TooltipTrigger>
                <TooltipContent side="right" align="center">{{ item.label }}</TooltipContent>
            </Tooltip>
        </nav>
    </aside>
</template>
