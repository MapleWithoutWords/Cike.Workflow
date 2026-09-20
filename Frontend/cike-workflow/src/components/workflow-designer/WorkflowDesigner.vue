<script setup lang="ts">
import { computed } from "vue"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import DesignerBreadcrumb from "./DesignerBreadcrumb.vue"
import DesignerCanvas from "./DesignerCanvas.vue"
import ActivityPalette from "./ActivityPalette.vue"
import PropertyPanel from "./PropertyPanel.vue"

const props = defineProps<{ designer: WorkflowDesignerState }>()

const selectedForPanel = computed(() => {
  const activity = props.designer.selectedActivity.value
  if (!activity) return null
  return { name: activity.name ?? activity.type, type: activity.type }
})
</script>

<template>
  <div class="flex h-full min-h-0 flex-col">
    <header class="flex shrink-0 items-center justify-between border-b px-4 py-2">
      <DesignerBreadcrumb
        :entries="designer.breadcrumb.value"
        @select="(index: number) => designer.drillStack.value = designer.drillStack.value.slice(0, index + 1)"
      />
    </header>
    <div class="flex min-h-0 flex-1">
      <ActivityPalette />
      <div class="relative min-w-0 flex-1">
        <DesignerCanvas
          :projection="designer.projection.value"
          :interactive="false"
          :selected-id="designer.selectedActivityId.value"
          @node-click="(id: string) => (designer.selectedActivityId.value = id || null)"
        />
        <div
          v-if="designer.loadError.value"
          class="absolute inset-x-0 top-0 border-b bg-destructive/10 px-4 py-2 text-xs text-destructive"
        >
          {{ designer.loadError.value }}
        </div>
      </div>
      <PropertyPanel :selected="selectedForPanel" />
    </div>
  </div>
</template>
