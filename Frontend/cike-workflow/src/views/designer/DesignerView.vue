<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useMediaQuery } from '@vueuse/core'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { ArrowLeft } from '@lucide/vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const isDesktopWide = useMediaQuery('(min-width: 1280px)')
const isBelowDesktop = useMediaQuery('(max-width: 1023px)')

const wsId = route.params.wsId as string
const defId = route.params.defId as string
</script>

<template>
  <div class="flex h-svh w-full flex-col bg-background">
    <!-- 设计器顶部条：返回定义列表 + 当前对象信息 -->
    <header
      class="flex h-14 shrink-0 items-center gap-3 border-b bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60"
    >
      <Button :as="RouterLink" :to="{ name: 'definitions', params: { wsId } }" variant="ghost" size="sm">
        <ArrowLeft class="mr-2 size-4" />
        返回定义列表
      </Button>
      <div class="flex items-center gap-2 text-sm text-muted-foreground">
        <span class="font-mono text-xs">{{ defId }}</span>
        <span class="text-muted-foreground/60">·</span>
        <span>设计器</span>
      </div>
      <div class="ml-auto text-xs text-muted-foreground">
        <!-- 设计器阶段未落地，本期不放假按钮 -->
        本期未实现
      </div>
    </header>

    <!-- 移动端降级提示（MASTER §7 <1024：设计器降级为只读预览） -->
    <div
      v-if="isBelowDesktop"
      role="status"
      class="border-b bg-warning/10 px-4 py-2 text-xs text-warning"
    >
      当前屏幕下设计器以只读预览呈现，请在桌面端编辑。
    </div>

    <!-- 三栏骨架：palette / canvas / inspector（≥1280 并排） -->
    <main class="flex flex-1 overflow-hidden">
      <section
        class="hidden w-64 shrink-0 border-r bg-muted/30 p-4 lg:block xl:w-72"
        aria-label="活动面板"
      >
        <h2 class="mb-3 text-sm font-semibold">活动</h2>
        <p class="text-xs text-muted-foreground">活动类型目录未暴露（MASTER §8），面板暂空。</p>
      </section>

      <section class="flex flex-1 flex-col overflow-hidden">
        <div class="flex flex-1 items-center justify-center bg-muted/10">
          <Card class="mx-auto max-w-md">
            <CardHeader>
              <CardTitle>设计器尚未落地</CardTitle>
              <CardDescription>
                画布、属性面板、问题呈现、留存/保存/发布门控将在 P4 阶段实现。
              </CardDescription>
            </CardHeader>
            <CardContent>
              <p class="text-xs text-muted-foreground">
                当前路由：<span class="font-mono">/w/{{ wsId }}/definitions/{{ defId }}/designer</span>
              </p>
            </CardContent>
          </Card>
        </div>
      </section>

      <section
        v-if="isDesktopWide"
        class="hidden w-80 shrink-0 border-l bg-muted/30 p-4 xl:block xl:w-96"
        aria-label="检查器"
      >
        <h2 class="mb-3 text-sm font-semibold">检查器</h2>
        <p class="text-xs text-muted-foreground">属性面板未落地，检查器暂空。</p>
      </section>
    </main>
  </div>
</template>
