<script setup lang="ts">
import { useRoute, useRouter } from "vue-router"
import { Upload, History, Pencil } from "@lucide/vue"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Card, CardContent } from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"

const route = useRoute()
const router = useRouter()
const definitionId = route.params.definitionId as string

// Mock data
const definition = {
  definitionId,
  name: "月度报销审批",
  description: "每月员工报销单据的逐级审批流程",
  type: "Workflow",
  version: 3,
  isLatest: true,
  isPublished: true,
  isReadonly: false,
  isSystem: false,
  usableAsActivity: false,
  materializerName: "Elsa.Workflows.Serialization",
}

const versions = [
  { version: 3, isLatest: true, isPublished: true, publishedNote: "增加多级审批", publishedAt: "2026-09-10" },
  { version: 2, isLatest: false, isPublished: false, publishedNote: "", publishedAt: "" },
  { version: 1, isLatest: false, isPublished: false, publishedNote: "初始版本", publishedAt: "2026-08-01" },
]
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-start justify-between">
      <div>
        <div class="flex items-center gap-2">
          <h1 class="text-xl font-semibold tracking-tight">{{ definition.name }}</h1>
          <Badge variant="secondary">{{ definition.type }}</Badge>
        </div>
        <p class="mt-1 font-mono text-xs text-muted-foreground">{{ definition.definitionId }}</p>
        <p class="mt-2 text-sm text-muted-foreground">{{ definition.description }}</p>
      </div>
      <div class="flex gap-2">
        <Button
          variant="outline"
          @click="router.push({ name: 'definition-designer', params: route.params })"
        >
          <Pencil :size="14" />
          编辑
        </Button>
        <Button>
          <Upload :size="14" />
          发布
        </Button>
      </div>
    </div>

    <!-- Info Grid -->
    <Card>
      <CardContent class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4 pt-6">
        <div>
          <p class="text-xs text-muted-foreground">当前版本</p>
          <p class="mt-1 font-mono text-sm">v{{ definition.version }}</p>
        </div>
        <div>
          <p class="text-xs text-muted-foreground">发布状态</p>
          <p class="mt-1 text-sm">
            <Badge v-if="definition.isPublished" class="bg-success/15 text-success border-transparent">已发布</Badge>
            <span v-else class="text-muted-foreground">未发布</span>
          </p>
        </div>
        <div>
          <p class="text-xs text-muted-foreground">可作为活动</p>
          <p class="mt-1 text-sm">{{ definition.usableAsActivity ? "是" : "否" }}</p>
        </div>
        <div>
          <p class="text-xs text-muted-foreground">Materializer</p>
          <p class="mt-1 truncate font-mono text-xs">{{ definition.materializerName }}</p>
        </div>
      </CardContent>
    </Card>

    <!-- Version History -->
    <div>
      <div class="mb-3 flex items-center gap-2">
        <History :size="16" class="text-muted-foreground" />
        <h2 class="font-medium">版本历史</h2>
      </div>
      <div class="rounded-lg border">
        <Table>
          <TableHeader>
            <TableRow class="bg-muted/50 hover:bg-muted/50">
              <TableHead>版本</TableHead>
              <TableHead>发布备注</TableHead>
              <TableHead>发布时间</TableHead>
              <TableHead class="text-right">操作</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableRow v-for="v in versions" :key="v.version">
              <TableCell>
                <span class="font-mono text-xs">v{{ v.version }}</span>
                <Badge v-if="v.isLatest" class="ml-2 bg-success/15 text-success border-transparent">最新</Badge>
                <Badge v-if="v.isPublished" class="ml-1 bg-info/15 text-info border-transparent">已发布</Badge>
              </TableCell>
              <TableCell class="text-muted-foreground">{{ v.publishedNote || "—" }}</TableCell>
              <TableCell class="text-xs text-muted-foreground">{{ v.publishedAt || "—" }}</TableCell>
              <TableCell class="text-right">
                <Button v-if="!v.isLatest" variant="ghost" size="sm">
                  回滚到此版本
                </Button>
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </div>
    </div>
  </div>
</template>
