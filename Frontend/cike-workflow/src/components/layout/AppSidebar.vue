<script setup lang="ts">
import { RouterLink, useRoute } from "vue-router"
import { LayoutDashboard, Boxes } from "@lucide/vue"
import { cn } from "@/lib/utils"

const route = useRoute()

const menuItems = [
  { label: "首页", to: "/", icon: LayoutDashboard, name: "home" },
  { label: "空间管理", to: "/workspaces", icon: Boxes, name: "workspaces" },
] as const

function isActive(name: string) {
  if (name === "home") return route.name === "home"
  // "workspaces" matches all workspace-related routes
  return route.path.startsWith("/workspaces") || route.name === name
}
</script>

<template>
  <aside class="flex w-56 shrink-0 flex-col border-r bg-sidebar">
    <!-- Logo -->
    <div class="flex h-14 items-center border-b px-4">
      <RouterLink to="/" class="flex items-center gap-2 font-semibold">
        <div class="flex h-7 w-7 items-center justify-center rounded-md bg-primary text-primary-foreground text-xs font-bold">
          CW
        </div>
        <span class="text-sm">Cike Workflow</span>
      </RouterLink>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 space-y-1 p-3">
      <RouterLink
        v-for="item in menuItems"
        :key="item.name"
        :to="item.to"
        :class="
          cn(
            'flex items-center gap-3 rounded-md px-3 py-2 text-sm transition-colors',
            isActive(item.name)
              ? 'bg-sidebar-accent text-sidebar-accent-foreground font-medium'
              : 'text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground',
          )
        "
      >
        <component :is="item.icon" :size="18" />
        {{ item.label }}
      </RouterLink>
    </nav>
  </aside>
</template>
