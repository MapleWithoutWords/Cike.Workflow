import type { RouteLocationRaw } from 'vue-router'
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useWorkspaceContext } from './useWorkspaceContext'

export interface BreadcrumbItem {
  label: string
  to?: RouteLocationRaw
}

/**
 * MASTER §4 面包屑规则：
 * 首页 › 工作空间 › {空间名} › {区} › {对象/目录路径}
 * 三级以上深度必须给面包屑。
 */
export function useBreadcrumbs() {
  const route = useRoute()
  const { inWorkspace, displayName, currentSection } = useWorkspaceContext()

  return computed<BreadcrumbItem[]>(() => {
    const items: BreadcrumbItem[] = [
      { label: '首页', to: { name: 'home' } },
    ]

    const name = route.name as string | undefined

    // 工作空间列表页
    if (name === 'workspaces') {
      items.push({ label: '工作空间' })
      return items
    }

    // 404
    if (name === 'not-found') {
      items.push({ label: '页面不存在' })
      return items
    }

    // 空间内：工作空间 › {空间名} › {区}
    if (inWorkspace.value) {
      items.push({ label: '工作空间', to: { name: 'workspaces' } })
      items.push({
        label: displayName.value,
        to: { name: 'workspace-detail', params: { wsId: displayName.value } },
      })

      if (currentSection.value === 'definitions') {
        items.push({ label: '工作流定义' })
      } else if (currentSection.value === 'instances') {
        items.push({ label: '工作流实例' })
      }
      return items
    }

    // 其他：用 route.meta.title 兜底
    const title = route.meta.title
    if (typeof title === 'string' && title.length > 0) {
      items.push({ label: title })
    }
    return items
  })
}
