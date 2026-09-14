<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import { Toaster } from '@/components/ui/sonner'
import AppLayout from '@/layouts/AppLayout.vue'
import BlankLayout from '@/layouts/BlankLayout.vue'

const route = useRoute()

const Layout = computed(() =>
  route.meta.layout === 'blank' ? BlankLayout : AppLayout,
)
</script>

<template>
  <component :is="Layout">
    <RouterView />
  </component>
  <Toaster position="top-right" :close-button="true" />
</template>
