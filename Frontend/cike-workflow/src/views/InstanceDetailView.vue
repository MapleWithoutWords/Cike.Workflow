<script setup lang="ts">
import { useRoute, RouterLink } from "vue-router"
import { ChevronRight } from "@lucide/vue"

const route = useRoute()
const instanceId = route.params.instanceId as string
const workspaceId = route.params.workspaceId as string

// Mock data
const instance = {
  id: instanceId,
  name: "报销审批 #1024",
  definitionName: "月度报销审批",
  definitionId: "d1",
  version: 3,
  correlationId: "abc-123-def",
  status: "Finished" as const,
  incidentCount: 0,
  isExecuting: false,
  finishedAt: "2026-09-15 10:30",
}

const activities = [
  { id: "a1", name: "开始", type: "Start", status: "Completed" as const, finishedAt: "10:00:01" },
  { id: "a2", name: "提交报销单", type: "HttpEndpoint", status: "Completed" as const, finishedAt: "10:00:05" },
  { id: "a3", name: "主管审批", type: "Approval", status: "Completed" as const, finishedAt: "10:15:30" },
  { id: "a4", name: "财务复核", type: "Approval", status: "Completed" as const, finishedAt: "10:28:00" },
  { id: "a5", name: "结束", type: "Finish", status: "Completed" as const, finishedAt: "10:30:00" },
]

const statusConfig: Record<string, { label: string; class: string }> = {
  Pending: { label: "等待", class: "bg-muted text-muted-foreground" },
  Running: { label: "运行中", class: "bg-info/15 text-info" },
  Completed: { label: "已完成", class: "bg-success/15 text-success" },
  Canceled: { label: "已取消", class: "bg-muted text-muted-foreground" },
  Faulted: { label: "故障", class: "bg-destructive/15 text-destructive" },
}

const instanceStatusConfig: Record<string, { label: string; class: string }> = {
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
  <div class="space-y-6">
    <!-- Header -->
    <div>
      <div class="flex items-center gap-3">
        <span :class="['inline-flex items-center rounded px-2 py-0.5 text-sm font-medium', instanceStatusConfig[instance.status]?.class]">
          {{ instanceStatusConfig[instance.status]?.label }}
        </span>
        <h1 class="text-xl font-semibold tracking-tight">{{ instance.name }}</h1>
      </div>
      <div class="mt-2 flex items-center gap-4 text-sm text-muted-foreground">
        <span>定义:
          <RouterLink :to="`/workspaces/${workspaceId}/definitions/${instance.definitionId}`" class="text-foreground hover:text-primary">
            {{ instance.definitionName }}
          </RouterLink>
        </span>
        <span>版本: <span class="font-mono">v{{ instance.version }}</span></span>
        <span>关联 ID: <span class="font-mono">{{ instance.correlationId }}</span></span>
      </div>
    </div>

    <!-- Activity Execution Records -->
    <div>
      <h2 class="mb-3 font-medium">活动执行记录</h2>
      <div class="rounded-lg border">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b bg-muted/50">
              <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">活动</th>
              <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">类型</th>
              <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">状态</th>
              <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">完成时间</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(act, idx) in activities"
              :key="act.id"
              class="border-b transition-colors last:border-b-0 hover:bg-muted/30"
            >
              <td class="px-4 py-2.5">
                <div class="flex items-center gap-2">
                  <span class="text-xs text-muted-foreground">{{ idx + 1 }}</span>
                  <ChevronRight v-if="idx < activities.length - 1" :size="12" class="text-muted-foreground/50" />
                  <span class="font-medium">{{ act.name }}</span>
                </div>
              </td>
              <td class="px-4 py-2.5 font-mono text-xs text-muted-foreground">{{ act.type }}</td>
              <td class="px-4 py-2.5">
                <span :class="['inline-flex items-center rounded px-1.5 py-0.5 text-xs', statusConfig[act.status]?.class]">
                  {{ statusConfig[act.status]?.label }}
                </span>
              </td>
              <td class="px-4 py-2.5 font-mono text-xs text-muted-foreground">{{ act.finishedAt }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
