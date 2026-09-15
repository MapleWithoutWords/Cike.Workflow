<script setup lang="ts">
import { Sun, Moon } from "@lucide/vue"
import { useTheme } from "@/composables/useTheme"

const { isDark, toggleTheme } = useTheme()
</script>

<template>
  <button
    type="button"
    class="inline-flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
    :aria-label="isDark ? '切换到浅色模式' : '切换到深色模式'"
    @click="toggleTheme()"
  >
    <Moon v-if="isDark" :size="16" />
    <Sun v-else :size="16" />
  </button>
</template>
