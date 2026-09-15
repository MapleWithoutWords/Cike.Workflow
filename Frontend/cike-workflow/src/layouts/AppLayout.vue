<script setup lang="ts">
import AppSidebar from "@/components/layout/AppSidebar.vue"
import AppHeader from "@/components/layout/AppHeader.vue"
</script>

<template>
  <div class="flex h-screen overflow-hidden">
    <AppSidebar />
    <div class="flex flex-1 flex-col overflow-hidden">
      <AppHeader />
      <main
        id="main-content"
        tabindex="-1"
        class="flex-1 overflow-y-auto p-6 outline-none"
      >
        <slot />
      </main>
    </div>
  </div>
</template>
