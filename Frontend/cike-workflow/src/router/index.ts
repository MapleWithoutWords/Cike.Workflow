import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'home',
    component: () => import('@/views/HomeView.vue'),
    meta: { title: '首页' },
  },
  {
    path: '/workspaces',
    name: 'workspaces',
    component: () => import('@/views/WorkspacesView.vue'),
    meta: { title: '工作空间' },
  },
  {
    path: '/w/:wsId',
    name: 'workspace-detail',
    component: () => import('@/views/WorkspaceDetailView.vue'),
    meta: { title: '空间详情', section: 'overview' },
  },
  {
    path: '/w/:wsId/definitions',
    name: 'definitions',
    component: () => import('@/views/DefinitionsView.vue'),
    meta: { title: '工作流定义', section: 'definitions' },
  },
  {
    path: '/w/:wsId/instances',
    name: 'instances',
    component: () => import('@/views/InstancesView.vue'),
    meta: { title: '工作流实例', section: 'instances' },
  },
  {
    path: '/w/:wsId/definitions/:defId/designer',
    name: 'designer',
    component: () => import('@/views/designer/DesignerView.vue'),
    meta: { title: '设计器', layout: 'blank' },
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/views/NotFoundView.vue'),
    meta: { title: '页面不存在' },
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior(to, _from, savedPosition) {
    if (savedPosition) return savedPosition
    if (to.hash) return { el: to.hash, behavior: 'smooth' }
    return { top: 0 }
  },
})

router.afterEach(() => {
  // MASTER §4: 路由切换后阅读焦点落在新内容起点
  requestAnimationFrame(() => {
    const main = document.getElementById('main-content')
    if (main instanceof HTMLElement) main.focus({ preventScroll: true })
  })
})

export default router
