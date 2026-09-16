import { computed } from "vue"
import { useRoute } from "vue-router"
import { getWorkspaceName } from "@/composables/useWorkspaceName"

export interface BreadcrumbItem {
  label: string
  to?: string
}

/**
 * 根据当前路由自动生成面包屑。
 * 结构：首页 › 空间管理 › [空间名] › [Tab/详情]
 */
export function useBreadcrumbs() {
  const route = useRoute()

  const items = computed<BreadcrumbItem[]>(() => {
    const crumbs: BreadcrumbItem[] = [{ label: "首页", to: "/" }]
    const name = route.name as string

    if (name === "home") return crumbs

    // 所有非首页路由都有 "空间管理" 层级
    crumbs.push({ label: "空间管理", to: "/workspaces" })

    const workspaceId = route.params.workspaceId as string | undefined
    const workspaceName = getWorkspaceName(workspaceId)

    if (!workspaceId) return crumbs

    // 空间层级（可切换节点由 AppBreadcrumb 单独处理）
    crumbs.push({
      label: workspaceName || workspaceId,
      to: `/workspaces/${workspaceId}/definitions`,
    })

    if (name === "definitions" || name === "definition-detail") {
      crumbs.push({
        label: "工作流定义",
        to: name === "definition-detail" ? `/workspaces/${workspaceId}/definitions` : undefined,
      })
      if (name === "definition-detail") {
        const defName = (route.meta.definitionName as string) ?? ""
        crumbs.push({ label: defName || "定义详情" })
      }
    } else if (name === "instances" || name === "instance-detail") {
      crumbs.push({
        label: "工作流实例",
        to: name === "instance-detail" ? `/workspaces/${workspaceId}/instances` : undefined,
      })
      if (name === "instance-detail") {
        const instName = (route.meta.instanceName as string) ?? ""
        crumbs.push({ label: instName || "实例详情" })
      }
    }

    return crumbs
  })

  return { items }
}
