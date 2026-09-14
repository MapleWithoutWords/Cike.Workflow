/**
 * router/index.ts
 *
 * Manual routes for ./src/pages/*.vue
 */

// Composables
import { createRouter, createWebHistory } from 'vue-router'
import Index from '@/pages/index.vue'

declare module 'vue-router' {
  interface RouteMeta {
    /** i18n key of the page title shown in the app bar */
    title?: string
    /** mdi icon; a route with title + icon shows in the sidebar */
    icon?: string
    /** i18n key of the sidebar group subheader; omit for ungrouped items */
    group?: string
  }
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: Index,
      meta: {
        title: 'layout.nav.home',
        icon: 'mdi-view-dashboard-outline',
      },
    },
    {
      path: '/workflow/definitions',
      name: 'workflow-definitions',
      component: () => import('@/pages/workflow/definitions.vue'),
      meta: {
        title: 'layout.nav.definitions',
        icon: 'mdi-source-branch',
        group: 'layout.nav.workflow',
      },
    },
    {
      path: '/workflow/instances',
      name: 'workflow-instances',
      component: () => import('@/pages/workflow/instances.vue'),
      meta: {
        title: 'layout.nav.instances',
        icon: 'mdi-play-circle-outline',
        group: 'layout.nav.workflow',
      },
    },
  ],
})

export default router
