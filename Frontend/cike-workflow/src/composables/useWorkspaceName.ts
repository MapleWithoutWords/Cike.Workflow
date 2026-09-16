import { ref } from "vue"

/**
 * 跨组件共享当前空间名称。
 *
 * 背景：面包屑（AppBreadcrumb，位于 layout 层）需要显示空间名称，
 * 但名称只有 WorkspaceDetailView 异步请求后才知道。二者不在同一
 * provide/inject 子树内，且直接 mutate `route.meta` 不会触发响应式
 * 更新（vue-router 的 meta 是 computed，仅在导航时重算）。
 * 因此用一个模块级 ref 作为共享状态。
 */
const currentWorkspaceId = ref("")
const currentWorkspaceName = ref("")

/** 由 WorkspaceDetailView 在获取到空间详情后写入。 */
export function setWorkspaceName(id: string, name: string) {
  currentWorkspaceId.value = id
  currentWorkspaceName.value = name
}

/**
 * 读取与给定 workspaceId 匹配的空间名称。
 * 仅当 id 匹配时返回，避免切换空间时短暂显示上一个空间的名称。
 */
export function getWorkspaceName(id: string | undefined): string {
  if (!id || id !== currentWorkspaceId.value) return ""
  return currentWorkspaceName.value
}
