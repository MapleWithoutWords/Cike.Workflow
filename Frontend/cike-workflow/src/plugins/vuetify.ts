/**
 * plugins/vuetify.ts
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Composables
import { createVuetify } from 'vuetify'
// Styles
import '@mdi/font/css/materialdesignicons.css'

import { catppuccinThemes } from './catppuccin'
import { initialThemePreference } from '@/stores/app'

import '../styles/layers.css'
import 'vuetify/styles'

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  theme: {
    // Start from the persisted preference; 'system' follows prefers-color-scheme
    defaultTheme: initialThemePreference(),
    themes: catppuccinThemes,
    // Circular reveal transition on theme switches (skipped when the
    // user prefers reduced motion); per-click origin via setTransitionOrigin
    transition: { duration: '400ms' },
  },
})
