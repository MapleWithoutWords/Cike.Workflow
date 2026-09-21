<script setup lang="ts">
import { computed, onMounted, ref } from "vue"
import { useRoute, useRouter } from "vue-router"
import { ArrowLeft } from "@lucide/vue"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import DesignerCanvas from "@/components/workflow-designer/DesignerCanvas.vue"
import { getApiV1WorkflowDefinitionsById, getApiV1WorkflowInstancesById } from "@/api/generated"
import type { WorkflowInstanceDetailDto, WorkflowStatus } from "@/api/generated"
import { fromWireActivity, type WireActivity } from "@/core/designer/serialization"
import { Flowchart } from "@/core/activities/Flowchart"
import { projectCanvas, projectFlowchart, type CanvasProjection } from "@/core/designer/projection"
import { buildActivityStatusMap } from "@/core/designer/execution"

const route = useRoute()
const router = useRouter()
const workspaceId = route.params.workspaceId as string
const instanceId = route.params.instanceId as string

const loading = ref(true)
const loadError = ref<string | null>(null)
const instance = ref<WorkflowInstanceDetailDto | null>(null)
const projection = ref<CanvasProjection>({ nodes: [], edges: [] })
const canvasRoot = ref<unknown>(null)

const instanceStatusUi: Record<WorkflowStatus, { label: string; class: string }> = {
  0: { label: "等待中", class: "bg-muted text-muted-foreground" },
  1: { label: "执行中", class: "bg-info/15 text-info" },
  2: { label: "已挂起", class: "bg-warning/15 text-warning" },
  3: { label: "已完成", class: "bg-success/15 text-success" },
  4: { label: "已取消", class: "bg-muted text-muted-foreground" },
  5: { label: "已故障", class: "bg-destructive/15 text-destructive" },
  6: { label: "已中断", class: "bg-warning/15 text-warning" },
}

const statusBadge = computed(() => {
  const status = instance.value?.status
  return status != null ? instanceStatusUi[status] : null
})

function goBack(): void {
  router.push({ name: "instances", params: { workspaceId } })
}

onMounted(async () => {
  loading.value = true
  loadError.value = null
  try {
    const { data, error } = await getApiV1WorkflowInstancesById({ path: { id: instanceId } })
    if (error || !data) {
      loadError.value = error ? String(error) : "加载实例失败"
      return
    }
    instance.value = data
    if (!data.definitionVersionId) {
      loadError.value = "实例缺少定义版本信息"
      return
    }
    const definition = await getApiV1WorkflowDefinitionsById({ path: { id: data.definitionVersionId } })
    if (definition.error || !definition.data) {
      loadError.value = definition.error ? String(definition.error) : "加载定义版本失败"
      return
    }
    const root = fromWireActivity((definition.data.root ?? {}) as WireActivity)
    canvasRoot.value = root
    const base =
      root instanceof Flowchart
        ? projectFlowchart(root)
        : "activities" in root && Array.isArray((root as { activities?: unknown }).activities)
          ? projectCanvas(root as never)
          : { nodes: [], edges: [] }
    // Overlay each activity's run status onto its node (matched by activityId).
    const statusMap = buildActivityStatusMap(data.activityInstances ?? [])
    projection.value = {
      edges: base.edges,
      nodes: base.nodes.map((node) => ({
        ...node,
        data: { ...node.data, status: statusMap.get(node.data.activityId) ?? null },
      })),
    }
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="flex h-full min-h-0 flex-col">
    <header class="flex shrink-0 items-center gap-3 border-b px-4 py-2">
      <Button variant="ghost" size="icon" title="返回" @click="goBack">
        <ArrowLeft :size="16" />
      </Button>
      <Badge v-if="statusBadge" :class="statusBadge.class">{{ statusBadge.label }}</Badge>
      <span class="truncate text-sm font-medium">{{ instance?.name || `实例 ${instanceId}` }}</span>
      <div class="flex items-center gap-4 text-xs text-muted-foreground">
        <RouterLink
          v-if="instance?.definitionVersionId"
          :to="`/workspaces/${workspaceId}/definitions/${instance.definitionVersionId}`"
          class="hover:text-primary"
        >
          {{ instance?.definitionName || "查看定义" }}
        </RouterLink>
        <span v-if="instance?.version">版本 <span class="font-mono">v{{ instance.version }}</span></span>
        <span v-if="instance?.correlationId">关联 ID <span class="font-mono">{{ instance.correlationId }}</span></span>
      </div>
    </header>

    <div class="relative min-h-0 flex-1">
      <div v-if="loading" class="flex h-full items-center justify-center text-sm text-muted-foreground">
        加载中…
      </div>
      <div v-else-if="loadError" class="flex h-full items-center justify-center text-sm text-destructive">
        {{ loadError }}
      </div>
      <DesignerCanvas
        v-else
        :projection="projection"
        :interactive="false"
        :selected-id="null"
        entry-key="instance"
        :entry-activity="canvasRoot"
      />
    </div>
  </div>
</template>
