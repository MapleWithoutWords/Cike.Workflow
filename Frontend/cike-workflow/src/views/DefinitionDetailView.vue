<script setup lang="ts">
import { useRoute, useRouter } from "vue-router"
import { Upload, History, Pencil } from "@lucide/vue"

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
          <span class="rounded bg-secondary px-1.5 py-0.5 text-xs">{{ definition.type }}</span>
        </div>
        <p class="mt-1 font-mono text-xs text-muted-foreground">{{ definition.definitionId }}</p>
        <p class="mt-2 text-sm text-muted-foreground">{{ definition.description }}</p>
      </div>
      <div class="flex gap-2">
        <button
          class="inline-flex h-9 items-center gap-2 rounded-md border px-3 text-sm transition-colors hover:bg-accent"
          @click="router.push({ name: 'definition-designer', params: route.params })"
        >
          <Pencil :size="14" />
          编辑
        </button>
        <button class="inline-flex h-9 items-center gap-2 rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:bg-primary/90">
          <Upload :size="14" />
          发布
        </button>
      </div>
    </div>

    <!-- Info Grid -->
    <div class="grid gap-4 rounded-lg border p-5 sm:grid-cols-2 lg:grid-cols-4">
      <div>
        <p class="text-xs text-muted-foreground">当前版本</p>
        <p class="mt-1 font-mono text-sm">v{{ definition.version }}</p>
      </div>
      <div>
        <p class="text-xs text-muted-foreground">发布状态</p>
        <p class="mt-1 text-sm">
          <span v-if="definition.isPublished" class="text-success">已发布</span>
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
    </div>

    <!-- Version History -->
    <div>
      <div class="mb-3 flex items-center gap-2">
        <History :size="16" class="text-muted-foreground" />
        <h2 class="font-medium">版本历史</h2>
      </div>
      <div class="rounded-lg border">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b bg-muted/50">
              <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">版本</th>
              <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">发布备注</th>
              <th class="px-4 py-2.5 text-left font-medium text-muted-foreground">发布时间</th>
              <th class="px-4 py-2.5 text-right font-medium text-muted-foreground">操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="v in versions" :key="v.version" class="border-b transition-colors last:border-b-0 hover:bg-muted/30">
              <td class="px-4 py-2.5">
                <span class="font-mono text-xs">v{{ v.version }}</span>
                <span v-if="v.isLatest" class="ml-2 rounded bg-success/15 px-1.5 py-0.5 text-xs text-success">最新</span>
                <span v-if="v.isPublished" class="ml-1 rounded bg-info/15 px-1.5 py-0.5 text-xs text-info">已发布</span>
              </td>
              <td class="px-4 py-2.5 text-muted-foreground">{{ v.publishedNote || "—" }}</td>
              <td class="px-4 py-2.5 text-xs text-muted-foreground">{{ v.publishedAt || "—" }}</td>
              <td class="px-4 py-2.5 text-right">
                <button v-if="!v.isLatest" class="text-xs text-muted-foreground hover:text-foreground">回滚到此版本</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
