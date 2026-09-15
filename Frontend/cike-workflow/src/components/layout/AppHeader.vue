<script setup lang="ts">
import AppBreadcrumb from "./AppBreadcrumb.vue"
import ThemeToggle from "./ThemeToggle.vue"
</script>

<template>
  <header class="flex h-14 shrink-0 items-center gap-4 border-b px-6">
    <div class="flex-1">
      <AppBreadcrumb />
    </div>
    <div class="flex items-center gap-2">
      <ThemeToggle />
    </div>
  </header>
</template>
