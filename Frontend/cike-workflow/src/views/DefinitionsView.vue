<script setup lang="ts">
import { RouterLink, useRoute } from "vue-router"
import { Folder, Workflow, Plus, Search } from "@lucide/vue"

const route = useRoute()
const workspaceId = route.params.workspaceId as string

// Mock data — mixed Folder + WorkflowDefinition items
const items = [
  { type: "folder" as const, id: "f1", name: "报销流程" },
  { type: "folder" as const, id: "f2", name: "审批中心" },
  { type: "definition" as const, id: "d1", name: "月度报销审批", definitionType: "Workflow" as const, version: 3, isLatest: true, publishedVersion: 2, isReadonly: false, isSystem: false },
  { type: "definition" as const, id: "d2", name: "年度预算审核", definitionType: "AgentWorkflow" as const, version: 1, isLatest: true, publishedVersion: null, isReadonly: false, isSystem: false },
  { type: "definition" as const, id: "d3", name: "采购审批流", definitionType: "Approval" as const, version: 5, isLatest: true, publishedVersion: 5, isReadonly: true, isSystem: true },
]

function typeBadgeClass(type: string) {
  switch (type) {
    case "Workflow": return "bg-secondary text-secondary-foreground"
    case "AgentWorkflow": return "bg-info/15 text-info"
    case "Approval": return "bg-warning/15 text-warning"
    default: return "bg-muted text-muted-foreground"
  }
}
</script>

<template>
  <div class="space-y-4">
    <!-- Toolbar -->
    <div class="flex items-center justify-between gap-4">
      <div class="relative max-w-sm flex-1">
        <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
        <input
          type="text"
          placeholder="搜索定义或目录..."
          class="h-9 w-full rounded-md border bg-background pl-9 pr-3 text-sm outline-none placeholder:text-muted-foreground focus:ring-2 focus:ring-ring"
        />
      </div>
      <div class="flex gap-2">
        <button class="inline-flex h-9 items-center gap-2 rounded-md border px-3 text-sm font-medium transition-colors hover:bg-accent">
          <Plus :size="16" />
          新建目录
        </button>
        <button class="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90">
          <Plus :size="16" />
          新建定义
        </button>
      </div>
    </div>

    <!-- Mixed List -->
    <div class="rounded-lg border">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b bg-muted/50">
            <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">名称</th>
            <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">类型</th>
            <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">版本</th>
            <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">发布状态</th>
            <th class="px-4 py-2.5 text-right font-medium text-muted-foreground">操作</th>
          </tr>
        </thead>
        <tbody>
          <!-- Folder rows -->
          <template v-for="item in items.filter(i => i.type === 'folder')" :key="item.id">
            <tr class="border-b transition-colors last:border-b-0 hover:bg-muted/30">
              <td class="px-4 py-2.5">
                <div class="flex items-center gap-2">
                  <Folder :size="16" class="text-warning" />
                  <span class="font-medium">{{ item.name }}</span>
                </div>
              </td>
              <td class="px-4 py-2.5 text-muted-foreground">目录</td>
              <td class="px-4 py-2.5" />
              <td class="px-4 py-2.5" />
              <td class="px-4 py-2.5 text-right text-muted-foreground">
                <button class="text-xs hover:text-foreground">重命名</button>
              </td>
            </tr>
          </template>
          <!-- Definition rows -->
          <template v-for="item in items.filter(i => i.type === 'definition')" :key="item.id">
            <tr class="border-b transition-colors last:border-b-0 hover:bg-muted/30">
              <td class="px-4 py-2.5">
                <RouterLink :to="`/workspaces/${workspaceId}/definitions/${item.id}`" class="flex items-center gap-2 hover:text-primary">
                  <Workflow :size="16" class="text-muted-foreground" />
                  <span class="font-medium">{{ item.name }}</span>
                  <span v-if="item.isSystem" class="rounded bg-muted px-1.5 py-0.5 text-xs text-muted-foreground">系统</span>
                  <span v-if="item.isReadonly" class="rounded bg-muted px-1.5 py-0.5 text-xs text-muted-foreground">只读</span>
                </RouterLink>
              </td>
              <td class="px-4 py-2.5">
                <span :class="['rounded px-1.5 py-0.5 text-xs', typeBadgeClass(item.definitionType)]">
                  {{ item.definitionType }}
                </span>
              </td>
              <td class="px-4 py-2.5 font-mono text-xs">
                v{{ item.version }}
                <span v-if="item.isLatest" class="ml-1 text-success">最新</span>
              </td>
              <td class="px-4 py-2.5">
                <span v-if="item.publishedVersion" class="text-success text-xs">已发布 v{{ item.publishedVersion }}</span>
                <span v-else class="text-xs text-muted-foreground">未发布</span>
              </td>
              <td class="px-4 py-2.5 text-right">
                <button class="text-xs text-muted-foreground hover:text-foreground">编辑</button>
              </td>
            </tr>
          </template>
        </tbody>
      </table>
    </div>
  </div>
</template>
