<script setup lang="ts">
import { ref, onMounted } from "vue"
import { RouterLink, useRoute } from "vue-router"
import { getApiV1Workspaces } from "@/api"
import type { WorkspaceItemDto } from "@/api"
import { setWorkspaceName } from "@/composables/useWorkspaceName"
import { cn } from "@/lib/utils"

const route = useRoute()
const workspaceId = route.params.workspaceId as string

const workspace = ref<WorkspaceItemDto | null>(null)

onMounted(async () => {
  const { data } = await getApiV1Workspaces({ query: { workspaceId } })
  workspace.value = data ?? null
  // 写入共享状态供面包屑显示空间名称
  if (data?.name) {
    setWorkspaceName(workspaceId, data.name)
  }
})

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
        {{ workspace?.name || "空间" }}
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
