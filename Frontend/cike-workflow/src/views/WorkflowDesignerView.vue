<script setup lang="ts">
import { onMounted } from "vue"
import { useRoute } from "vue-router"
import { useWorkflowDesigner } from "@/composables/useWorkflowDesigner"
import WorkflowDesigner from "@/components/workflow-designer/WorkflowDesigner.vue"

const route = useRoute()
const designer = useWorkflowDesigner()

onMounted(() => {
  designer.load(route.params.definitionId as string)
})
</script>

<template>
  <div class="h-full min-h-0">
    <div v-if="designer.loading.value" class="flex h-full items-center justify-center text-sm text-muted-foreground">
      加载中…
    </div>
    <WorkflowDesigner v-else :designer="designer" />
  </div>
</template>
