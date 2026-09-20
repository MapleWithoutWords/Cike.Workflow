<script setup lang="ts">
// Placeholder: the property panel and type→form mapping arrive with their own ticket.
defineProps<{
  selected: { name: string; type: string } | null
}>()
</script>

<template>
  <aside class="flex w-72 shrink-0 flex-col border-l">
    <div class="border-b px-3 py-2 text-xs font-medium text-muted-foreground">属性</div>
    <div v-if="!selected" class="px-3 py-6 text-center text-xs text-muted-foreground">未选中节点</div>
    <div v-else class="space-y-2 px-3 py-3">
      <div class="text-sm font-medium text-foreground">{{ selected.name }}</div>
      <div class="font-mono text-xs text-muted-foreground">{{ selected.type }}</div>
    </div>
  </aside>
</template>
