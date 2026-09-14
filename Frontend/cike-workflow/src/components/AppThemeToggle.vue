<template>
  <v-btn
    icon
    :aria-label="t('theme.toggle')"
    :title="t('theme.toggle')"
    @click="toggle($event)"
  >
    <v-icon :icon="isDark ? 'mdi-weather-sunny' : 'mdi-weather-night'" />
  </v-btn>
</template>

<script lang="ts" setup>
  import { useI18n } from 'vue-i18n'
  import { useAppTheme } from '@/composables/useAppTheme'

  const { t } = useI18n()
  const { isDark, toggle } = useAppTheme()
</script>
