<script setup lang="ts">
import { RouterLink, useRoute } from "vue-router"
import { useWorkspaceContext } from "@/composables/useWorkspaceContext"
import { cn } from "@/lib/utils"

const route = useRoute()
const { workspaceId, workspaceName } = useWorkspaceContext()

const tabs = [
  { label: "工作流定义", routeName: "definitions" },
  { label: "工作流实例", routeName: "instances" },
]

function isTabActive(routeName: string) {
  const name = route.name as string
  if (routeName === "definitions") return name === "definitions" || name === "definition-detail"
  if (routeName === "instances") return name === "instances" || name === "instance-detail"
  return name === routeName
}
</script>

<template>
  <div class="space-y-4">
    <!-- Workspace Header -->
    <div>
      <h1 class="text-xl font-semibold tracking-tight">
        {{ workspaceName || "空间" }}
      </h1>
    </div>

    <!-- Tabs -->
    <div class="border-b">
      <nav class="flex gap-6">
        <RouterLink
          v-for="tab in tabs"
          :key="tab.routeName"
          :to="{ name: tab.routeName, params: { workspaceId } }"
          :class="
            cn(
              'border-b-2 px-1 pb-2.5 text-sm font-medium transition-colors',
              isTabActive(tab.routeName)
                ? 'border-primary text-foreground'
                : 'border-transparent text-muted-foreground hover:text-foreground',
            )
          "
        >
          {{ tab.label }}
        </RouterLink>
      </nav>
    </div>

    <!-- Tab Content -->
    <RouterView />
  </div>
</template>
