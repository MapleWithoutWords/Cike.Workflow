<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { RouterLink } from 'vue-router'
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '@/components/ui/breadcrumb'
import { useBreadcrumbs } from '@/composables/useBreadcrumbs'
import { cn } from '@/lib/utils'

defineProps<{ class?: HTMLAttributes['class'] }>()

const items = useBreadcrumbs()
</script>

<template>
  <Breadcrumb :class="cn('overflow-hidden', $props.class)">
    <BreadcrumbList class="flex-nowrap overflow-x-auto whitespace-nowrap">
      <template v-for="(item, idx) in items" :key="idx">
        <BreadcrumbItem>
          <BreadcrumbLink v-if="item.to" :as-child="true">
            <RouterLink :to="item.to">{{ item.label }}</RouterLink>
          </BreadcrumbLink>
          <BreadcrumbPage v-else>{{ item.label }}</BreadcrumbPage>
        </BreadcrumbItem>
        <BreadcrumbSeparator v-if="idx < items.length - 1" />
      </template>
    </BreadcrumbList>
  </Breadcrumb>
</template>
