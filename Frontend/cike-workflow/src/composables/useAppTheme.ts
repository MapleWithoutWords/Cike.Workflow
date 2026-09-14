import type { ThemePreference } from '@/stores/app'
import { computed } from 'vue'
import { useTheme } from 'vuetify'
import { useAppStore } from '@/stores/app'

/**
 * Glue between the persisted theme preference (app store) and Vuetify's
 * theme instance. Side-effect free per call — boot-time preference is
 * applied via `defaultTheme` in the vuetify plugin; runtime switches go
 * through setPreference/toggle, which update store and theme together.
 */
export function useAppTheme() {
  const theme = useTheme()
  const app = useAppStore()

  const isDark = computed(() => theme.current.value.dark)
  const preference = computed(() => app.themePreference)

  function setPreference(preference: ThemePreference) {
    app.setThemePreference(preference)
    theme.change(preference)
  }

  /** Toggle light/dark; pass the triggering event to expand the theme
   *  transition from that point (`origin`), falling back to top-center. */
  function toggle(origin?: PointerEvent | Element | null) {
    theme.setTransitionOrigin(origin ?? null)
    setPreference(isDark.value ? 'light' : 'dark')
  }

  return { theme, isDark, preference, setPreference, toggle }
}
