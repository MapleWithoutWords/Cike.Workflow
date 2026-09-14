/**
 * plugins/catppuccin.ts
 *
 * Catppuccin palette tokens + Vuetify theme definitions.
 * Source of truth: catppuccin/palette v1.8.0 (https://github.com/catppuccin/palette)
 *
 * Roles:
 * - Mocha → theme `dark`, Latte → theme `light`. Both flavors share the same
 *   14 accents, so light/dark stay visually consistent.
 * - All accents are registered as theme colors, so every one gets a
 *   `--v-theme-<name>` CSS variable and works with component `color` props.
 *   Workflow type badges / instance states map onto them:
 *   teal = normal workflow, mauve = agent workflow, peach = approval flow,
 *   sky = running, green = succeeded, red = failed.
 */

import type { ThemeDefinition } from 'vuetify'

/** Latte flavor — light */
const latte = {
  rosewater: '#dc8a78',
  flamingo: '#dd7878',
  pink: '#ea76cb',
  mauve: '#8839ef',
  red: '#d20f39',
  maroon: '#e64553',
  peach: '#fe640b',
  yellow: '#df8e1d',
  green: '#40a02b',
  teal: '#179299',
  sky: '#04a5e5',
  sapphire: '#209fb5',
  blue: '#1e66f5',
  lavender: '#7287fd',
  text: '#4c4f69',
  subtext1: '#5c5f77',
  subtext0: '#6c6f85',
  overlay2: '#7c7f93',
  overlay1: '#8c8fa1',
  overlay0: '#9ca0b0',
  surface2: '#acb0be',
  surface1: '#bcc0cc',
  surface0: '#ccd0da',
  base: '#eff1f5',
  mantle: '#e6e9ef',
  crust: '#dce0e8',
} as const

/** Mocha flavor — dark */
const mocha = {
  rosewater: '#f5e0dc',
  flamingo: '#f2cdcd',
  pink: '#f5c2e7',
  mauve: '#cba6f7',
  red: '#f38ba8',
  maroon: '#eba0ac',
  peach: '#fab387',
  yellow: '#f9e2af',
  green: '#a6e3a1',
  teal: '#94e2d5',
  sky: '#89dceb',
  sapphire: '#74c7ec',
  blue: '#89b4fa',
  lavender: '#b4befe',
  text: '#cdd6f4',
  subtext1: '#bac2de',
  subtext0: '#a6adc8',
  overlay2: '#9399b2',
  overlay1: '#7f849c',
  overlay0: '#6c7086',
  surface2: '#585b70',
  surface1: '#45475a',
  surface0: '#313244',
  base: '#1e1e2e',
  mantle: '#181825',
  crust: '#11111b',
} as const

type Flavor = Record<keyof typeof mocha, string>

/**
 * Accent contrast: Mocha accents are pale pastels → crust text on top;
 * Latte accents are deep → white text on top.
 */
function buildTheme (flavor: Flavor, dark: boolean): ThemeDefinition {
  const onAccent = dark ? flavor.crust : '#ffffff'

  return {
    dark,
    colors: {
      // Vuetify core roles
      primary: flavor.mauve,
      'on-primary': onAccent,
      secondary: flavor.lavender,
      'on-secondary': onAccent,
      accent: flavor.pink,
      'on-accent': onAccent,
      background: flavor.base,
      'on-background': flavor.text,
      surface: dark ? flavor.surface0 : '#ffffff',
      'on-surface': flavor.text,
      'surface-variant': dark ? flavor.surface1 : flavor.surface0,
      'on-surface-variant': flavor.subtext0,
      'surface-bright': dark ? flavor.surface2 : flavor.surface1,
      'on-surface-bright': flavor.text,
      error: flavor.red,
      'on-error': onAccent,
      warning: flavor.yellow,
      'on-warning': onAccent,
      success: flavor.green,
      'on-success': onAccent,
      info: flavor.sky,
      'on-info': onAccent,

      // Catppuccin accents — full set for CSS vars (`--v-theme-<name>`)
      // and component `color` props; also the pool for per-definition
      // identity colors assigned by business code.
      rosewater: flavor.rosewater,
      flamingo: flavor.flamingo,
      pink: flavor.pink,
      mauve: flavor.mauve,
      maroon: flavor.maroon,
      peach: flavor.peach,
      yellow: flavor.yellow,
      green: flavor.green,
      teal: flavor.teal,
      sky: flavor.sky,
      sapphire: flavor.sapphire,
      blue: flavor.blue,
      lavender: flavor.lavender,

      // Extra semantic layers
      crust: flavor.crust,
      mantle: flavor.mantle,
      overlay: flavor.overlay0,
      muted: flavor.subtext0,
    },
  }
}

export const catppuccinThemes: Record<'dark' | 'light', ThemeDefinition> = {
  dark: buildTheme(mocha, true),
  light: buildTheme(latte, false),
}
