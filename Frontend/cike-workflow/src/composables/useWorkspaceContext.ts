import { computed } from "vue"
import { useRoute } from "vue-router"
import { getWorkspaceName } from "@/mock/workspaces"

/**
 * 从当前路由参数中读取工作空间上下文。
 * 空间 ID 由路由层注入，无需全局 store——路由即状态。
 */
export function useWorkspaceContext() {
  const route = useRoute()

  const workspaceId = computed<string | null>(
    () => (route.params.workspaceId as string) ?? null,
  )

  /**
   * 显示名称。优先取路由 meta 注入的名称，
   * 否则按 id 从 mock 数据查找（接入 API 后改为请求结果）。
   */
  const workspaceName = computed<string>(
    () =>
      (route.meta.workspaceName as string) ||
      getWorkspaceName(workspaceId.value),
  )

  return { workspaceId, workspaceName }
}
