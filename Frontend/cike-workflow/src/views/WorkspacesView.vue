<script setup lang="ts">
import { RouterLink } from "vue-router"
import { Plus, Search, Boxes } from "@lucide/vue"
import { mockWorkspaces as workspaces } from "@/mock/workspaces"
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold tracking-tight">空间管理</h1>
        <p class="mt-1 text-sm text-muted-foreground">
          工作空间用于划分工作流的管理范围
        </p>
      </div>
      <button class="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90">
        <Plus :size="16" />
        新建空间
      </button>
    </div>

    <!-- Search -->
    <div class="relative max-w-sm">
      <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
      <input
        type="text"
        placeholder="搜索空间..."
        class="h-9 w-full rounded-md border bg-background pl-9 pr-3 text-sm outline-none placeholder:text-muted-foreground focus:ring-2 focus:ring-ring"
      />
    </div>

    <!-- Workspace Cards -->
    <div v-if="workspaces.length" class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <RouterLink
        v-for="ws in workspaces"
        :key="ws.id"
        :to="`/workspaces/${ws.id}`"
        class="group rounded-lg border p-5 transition-colors hover:border-primary/50 hover:bg-accent/30"
      >
        <div class="flex items-start justify-between">
          <div class="flex h-10 w-10 items-center justify-center rounded-md bg-primary/10 text-primary">
            <Boxes :size="20" />
          </div>
          <span class="rounded bg-muted px-2 py-0.5 font-mono text-xs text-muted-foreground">
            {{ ws.code }}
          </span>
        </div>
        <h3 class="mt-3 font-medium">{{ ws.name }}</h3>
        <p class="mt-1 line-clamp-2 text-sm text-muted-foreground">
          {{ ws.description }}
        </p>
        <p class="mt-3 text-xs text-muted-foreground">
          更新于 {{ ws.updatedAt }}
        </p>
      </RouterLink>
    </div>

    <!-- Empty State -->
    <div v-else class="flex flex-col items-center justify-center rounded-lg border border-dashed py-16">
      <Boxes :size="40" class="text-muted-foreground/50" />
      <h3 class="mt-4 font-medium">暂无工作空间</h3>
      <p class="mt-1 text-sm text-muted-foreground">
        创建一个工作空间来开始管理你的工作流
      </p>
      <button class="mt-4 inline-flex h-9 items-center gap-2 rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground hover:bg-primary/90">
        <Plus :size="16" />
        新建空间
      </button>
    </div>
  </div>
</template>
