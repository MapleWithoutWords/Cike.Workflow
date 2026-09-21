<script setup lang="ts">
import { RouterLink, useRoute } from "vue-router"
import { Search, Filter } from "@lucide/vue"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"

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
        <Input
          placeholder="搜索实例名称或关联 ID..."
          class="pl-9"
        />
      </div>
      <Button variant="outline">
        <Filter :size="16" />
        状态筛选
      </Button>
    </div>

    <!-- Instance Table -->
    <div class="rounded-lg border">
      <Table>
        <TableHeader>
          <TableRow class="bg-muted/50 hover:bg-muted/50">
            <TableHead>状态</TableHead>
            <TableHead>名称 / 定义</TableHead>
            <TableHead>版本</TableHead>
            <TableHead>关联 ID</TableHead>
            <TableHead>异常</TableHead>
            <TableHead>完成时间</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          <TableRow
            v-for="inst in instances"
            :key="inst.id"
          >
            <TableCell>
              <Badge :class="statusConfig[inst.status]?.class">
                {{ statusConfig[inst.status]?.label ?? inst.status }}
              </Badge>
            </TableCell>
            <TableCell>
              <RouterLink :to="`/workspaces/${workspaceId}/instances/${inst.id}`" class="hover:text-primary">
                <span class="font-medium">{{ inst.name || inst.definitionName }}</span>
                <span v-if="inst.name" class="ml-1 text-xs text-muted-foreground">({{ inst.definitionName }})</span>
              </RouterLink>
            </TableCell>
            <TableCell class="font-mono text-xs">v{{ inst.version }}</TableCell>
            <TableCell class="font-mono text-xs text-muted-foreground">{{ inst.correlationId }}</TableCell>
            <TableCell>
              <Badge v-if="inst.incidentCount > 0" variant="destructive">
                {{ inst.incidentCount }}
              </Badge>
              <span v-else class="text-muted-foreground">—</span>
            </TableCell>
            <TableCell class="text-xs text-muted-foreground">{{ inst.finishedAt || "—" }}</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </div>
  </div>
</template>
