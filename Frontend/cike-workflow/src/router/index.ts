import { createRouter, createWebHistory } from "vue-router"
import type { RouteRecordRaw } from "vue-router"

const routes: RouteRecordRaw[] = [
  {
    path: "/",
    name: "home",
    component: () => import("@/views/HomeView.vue"),
    meta: { title: "首页" },
  },
  {
    path: "/workspaces",
    name: "workspaces",
    component: () => import("@/views/WorkspacesView.vue"),
    meta: { title: "空间管理" },
  },
  {
    path: "/workspaces/:workspaceId",
    name: "workspace-detail",
    component: () => import("@/views/WorkspaceDetailView.vue"),
    redirect: (to) => ({
      name: "definitions",
      params: { workspaceId: to.params.workspaceId },
    }),
    children: [
      {
        path: "definitions",
        name: "definitions",
        component: () => import("@/views/DefinitionsView.vue"),
        meta: { title: "工作流定义" },
      },
      {
        path: "definitions/:definitionId",
        name: "definition-detail",
        component: () => import("@/views/DefinitionDetailView.vue"),
        meta: { title: "定义详情" },
      },
      {
        path: "instances",
        name: "instances",
        component: () => import("@/views/InstancesView.vue"),
        meta: { title: "工作流实例" },
      },
      {
        path: "instances/:instanceId",
        name: "instance-detail",
        component: () => import("@/views/InstanceDetailView.vue"),
        meta: { title: "实例详情" },
      },
    ],
  },
  {
    path: "/:pathMatch(.*)*",
    name: "not-found",
    component: () => import("@/views/NotFoundView.vue"),
    meta: { title: "页面未找到" },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(_to, _from, savedPosition) {
    if (savedPosition) return savedPosition
    return { top: 0 }
  },
})

router.afterEach((to) => {
  const title = to.meta.title as string | undefined
  document.title = title ? `${title} - Cike Workflow` : "Cike Workflow"
  // Move focus to main content for keyboard users after navigation
  const main = document.getElementById("main-content")
  main?.focus({ preventScroll: true })
})

export default router
