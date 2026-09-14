<template>
  <v-navigation-drawer v-model="drawer" :rail="isRail" :permanent="mdAndUp" :width="256" rail-width="72">
    <AppLogo :rail="isRail" />

    <v-list nav>
      <template v-for="section in navSections" :key="section.titleKey ?? 'default'">
        <v-list-subheader v-if="section.titleKey && !isRail">
          {{ t(section.titleKey) }}
        </v-list-subheader>

        <v-list-item v-for="item in section.items" :key="item.to" :to="item.to" :prepend-icon="item.icon"
          :title="t(item.titleKey)" rounded="lg">
          <v-tooltip v-if="isRail" activator="parent" location="end" :text="t(item.titleKey)" />
        </v-list-item>
      </template>
    </v-list>
  </v-navigation-drawer>

  <v-app-bar border>
    <v-app-bar-nav-icon :icon="navIcon" :aria-label="t('layout.toggleNav')" :title="t('layout.toggleNav')"
      @click="toggleNav" />

    <v-app-bar-title>{{ pageTitle }}</v-app-bar-title>

    <template #append>
      <AppThemeToggle />
    </template>
  </v-app-bar>

  <v-main>
    <router-view />
  </v-main>
</template>

<script setup lang="ts">
import type { RouteRecordRaw } from 'vue-router'
import { storeToRefs } from 'pinia'
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useDisplay } from 'vuetify'
import AppLogo from '@/components/AppLogo.vue'
import AppThemeToggle from '@/components/AppThemeToggle.vue'
import { useAppStore } from '@/stores/app'

interface NavItem {
  titleKey: string
  icon: string
  to: string
}

interface NavSection {
  /** i18n key of the group subheader; omit for ungrouped items */
  titleKey?: string
  items: NavItem[]
}

/** Derive sidebar sections from top-level route records: meta.icon + meta.title marks a nav item, meta.group its section. */
function buildNavSections(routes: readonly RouteRecordRaw[]): NavSection[] {
  const sections: NavSection[] = []
  for (const { path, meta } of routes) {
    const { title, icon, group } = meta ?? {}
    if (!title || !icon) continue
    const item: NavItem = { titleKey: title, icon, to: path }
    const section = sections.find(s => (s.titleKey ?? '') === (group ?? ''))
    if (section) section.items.push(item)
    else sections.push({ titleKey: group, items: [item] })
  }
  return sections
}

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const { mdAndUp } = useDisplay()

console.log('router.options.routes', router.options.routes)
const navSections = buildNavSections(router.options.routes)

const appStore = useAppStore()
const { sidebarRail } = storeToRefs(appStore)

// Start as null so VNavigationDrawer self-initializes per viewport
// (permanent drawer open on desktop, closed overlay on mobile); desktop uses rail collapse instead
const drawer = ref<boolean | null>(null)

const isRail = computed(() => mdAndUp.value && sidebarRail.value)

const navIcon = computed(() => (mdAndUp.value && !sidebarRail.value ? 'mdi-menu-open' : 'mdi-menu'))

function toggleNav() {
  if (mdAndUp.value) {
    appStore.setSidebarRail(!sidebarRail.value)
  } else {
    drawer.value = !drawer.value
  }
}

const pageTitle = computed(() => (route.meta.title ? t(route.meta.title) : t('app.title')))
</script>
