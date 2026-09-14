<template>
  <v-empty-state
    :icon="icon"
    :title
    :text="t('pages.pending')"
    size="128"
  />
</template>

<script setup lang="ts">
  import { useI18n } from 'vue-i18n'

  defineProps<{
    icon?: string
    title: string
  }>()

  const { t } = useI18n()
</script>
