import { useColorMode } from '@vueuse/core'
import { computed } from 'vue'

export function useTheme() {
  const mode = useColorMode({
    selector: 'html',
    attribute: 'class',
    emitAuto: true,
    storageKey: 'theme-preference',
  })

  const isDark = computed(() => mode.value === 'dark')

  function toggle() {
    mode.value = isDark.value ? 'light' : 'dark'
  }

  return { mode, isDark, toggle }
}
