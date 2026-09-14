// Utilities
import { defineStore } from 'pinia'

export type ThemePreference = 'light' | 'dark' | 'system'

const THEME_STORAGE_KEY = 'app.theme-preference'
const RAIL_STORAGE_KEY = 'app.sidebar-rail'

function readThemePreference (): ThemePreference {
  const stored = localStorage.getItem(THEME_STORAGE_KEY)
  return stored === 'light' || stored === 'dark' || stored === 'system' ? stored : 'system'
}

/**
 * Read the persisted preference before Pinia/Vuetify mount, so the vuetify
 * plugin can use it as `defaultTheme` and avoid a flash of the wrong theme.
 */
export function initialThemePreference (): ThemePreference {
  try {
    return readThemePreference()
  } catch {
    return 'system'
  }
}

/**
 * Read the persisted rail preference before Pinia mounts, mirroring
 * initialThemePreference.
 */
export function initialSidebarRail (): boolean {
  try {
    return localStorage.getItem(RAIL_STORAGE_KEY) === 'true'
  } catch {
    return false
  }
}

export const useAppStore = defineStore('app', {
  state: () => ({
    themePreference: initialThemePreference(),
    sidebarRail: initialSidebarRail(),
  }),
  actions: {
    setThemePreference (preference: ThemePreference) {
      this.themePreference = preference
      localStorage.setItem(THEME_STORAGE_KEY, preference)
    },
    setSidebarRail (rail: boolean) {
      this.sidebarRail = rail
      localStorage.setItem(RAIL_STORAGE_KEY, String(rail))
    },
  },
})
