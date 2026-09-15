<script setup lang="ts">
import { RouterLink } from "vue-router"
import { ChevronRight } from "@lucide/vue"
import { useBreadcrumbs } from "@/composables/useBreadcrumbs"

const { items } = useBreadcrumbs()
</script>

<template>
  <nav aria-label="面包屑导航">
    <ol class="flex items-center gap-1 text-sm">
      <template v-for="(item, index) in items" :key="index">
        <li class="flex items-center">
          <RouterLink
            v-if="item.to"
            :to="item.to"
            class="rounded px-1.5 py-0.5 text-muted-foreground transition-colors hover:text-foreground"
          >
            {{ item.label }}
          </RouterLink>
          <span v-else class="rounded px-1.5 py-0.5 text-foreground font-medium">
            {{ item.label }}
          </span>
        </li>
        <li v-if="index < items.length - 1" class="flex items-center">
          <ChevronRight :size="14" class="text-muted-foreground" />
        </li>
      </template>
    </ol>
  </nav>
</template>
