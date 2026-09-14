<template>
  <router-link class="app-logo" :class="{ 'app-logo--rail': rail }" to="/" aria-label="Home">
    <v-icon icon="mdi-graph-outline" color="primary" size="28" />
    <span v-if="!rail" class="app-logo__title text-h6 font-weight-bold">{{ t('app.title') }}</span>
  </router-link>
</template>

<script setup lang="ts">
  import { useI18n } from 'vue-i18n'

  defineProps<{
    /** Rail mode: icon only, centered */
    rail?: boolean
  }>()

  const { t } = useI18n()
</script>

<style scoped>
  .app-logo {
    display: flex;
    align-items: center;
    gap: 10px;
    height: 64px;
    padding-inline: 18px;
    text-decoration: none;
  }

  .app-logo--rail {
    justify-content: center;
    padding-inline: 0;
  }
</style>
