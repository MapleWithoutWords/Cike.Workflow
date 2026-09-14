<template>
  <v-container class="py-6">
    <PagePlaceholder icon="mdi-play-circle-outline" :title="t('layout.nav.instances')" />
  </v-container>
</template>

<script setup lang="ts">
  import { useI18n } from 'vue-i18n'
  import PagePlaceholder from '@/components/PagePlaceholder.vue'

  const { t } = useI18n()
</script>
