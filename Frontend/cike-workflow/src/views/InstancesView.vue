<script setup lang="ts">
import { RouterLink, useRoute } from "vue-router"
import { Search, Filter } from "@lucide/vue"

const route = useRoute()
const workspaceId = route.params.workspaceId as string

// Mock data
const instances = [
  { id: "i1", name: "报销审批 #1024", definitionName: "月度报销审批", version: 3, correlationId: "abc-123-def", status: "Finished" as const, incidentCount: 0, isExecuting: false, finishedAt: "2026-09-15 10:30" },
  { id: "i2", name: "", definitionName: "年度预算审核", version: 1, correlationId: "xyz-456-ghi", status: "Faulted" as const, incidentCount: 2, isExecuting: false, finishedAt: "2026-09-14 16:45" },
  { id: "i3", name: "采购审批 #88", definitionName: "采购审批流", version: 5, correlationId: "jkl-789-mno", status: "Executing" as const, incidentCount: 0, isExecuting: true, finishedAt: "" },
  { id: "i4", name: "", definitionName: "月度报销审批", version: 3, correlationId: "pqr-012-stu", status: "Suspended" as const, incidentCount: 1, isExecuting: false, finishedAt: "" },
  { id: "i5", name: "", definitionName: "采购审批流", version: 4, correlationId: "vwx-345-yza", status: "Cancelled" as const, incidentCount: 0, isExecuting: false, finishedAt: "2026-09-13 09:00" },
]

const statusConfig: Record<string, { label: string; class: string }> = {
  Pending: { label: "等待中", class: "bg-muted text-muted-foreground" },
  Executing: { label: "执行中", class: "bg-info/15 text-info" },
  Suspended: { label: "已挂起", class: "bg-warning/15 text-warning" },
  Finished: { label: "已完成", class: "bg-success/15 text-success" },
  Cancelled: { label: "已取消", class: "bg-muted text-muted-foreground" },
  Faulted: { label: "已故障", class: "bg-destructive/15 text-destructive" },
  Interrupted: { label: "已中断", class: "bg-warning/15 text-warning" },
}
</script>

<template>
  <div class="space-y-4">
    <!-- Filters -->
    <div class="flex items-center gap-3">
      <div class="relative max-w-sm flex-1">
        <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
        <input
          type="text"
          placeholder="搜索实例名称或关联 ID..."
          class="h-9 w-full rounded-md border bg-background pl-9 pr-3 text-sm outline-none placeholder:text-muted-foreground focus:ring-2 focus:ring-ring"
        />
      </div>
      <button class="inline-flex h-9 items-center gap-2 rounded-md border px-3 text-sm transition-colors hover:bg-accent">
        <Filter :size="16" />
        状态筛选
      </button>
    </div>

    <!-- Instance Table -->
    <div class="rounded-lg border">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b bg-muted/50">
            <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">状态</th>
            <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">名称 / 定义</th>
            <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">版本</th>
            <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">关联 ID</th>
            <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">异常</th>
            <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">完成时间</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="inst in instances"
            :key="inst.id"
            class="border-b transition-colors last:border-b-0 hover:bg-muted/30"
          >
            <td class="px-4 py-2.5">
              <span :class="['inline-flex items-center rounded px-1.5 py-0.5 text-xs font-medium', statusConfig[inst.status]?.class]">
                {{ statusConfig[inst.status]?.label ?? inst.status }}
              </span>
            </td>
            <td class="px-4 py-2.5">
              <RouterLink :to="`/workspaces/${workspaceId}/instances/${inst.id}`" class="hover:text-primary">
                <span class="font-medium">{{ inst.name || inst.definitionName }}</span>
                <span v-if="inst.name" class="ml-1 text-xs text-muted-foreground">({{ inst.definitionName }})</span>
              </RouterLink>
            </td>
            <td class="px-4 py-2.5 font-mono text-xs">v{{ inst.version }}</td>
            <td class="px-4 py-2.5 font-mono text-xs text-muted-foreground">{{ inst.correlationId }}</td>
            <td class="px-4 py-2.5">
              <span v-if="inst.incidentCount > 0" class="inline-flex items-center rounded bg-destructive/15 px-1.5 py-0.5 text-xs text-destructive">
                {{ inst.incidentCount }}
              </span>
              <span v-else class="text-muted-foreground">—</span>
            </td>
            <td class="px-4 py-2.5 text-xs text-muted-foreground">{{ inst.finishedAt || "—" }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
