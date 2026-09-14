import { computed } from 'vue'
import { useRoute } from 'vue-router'

/**
 * 空间上下文（MASTER §4 L2 侧栏的"当前空间"事实）。
 *
 * P0 阶段 displayName 降级为 wsId（后端尚无接口），
 * P1 接入真实 API 后在此单点替换，侧栏/面包屑自动同步。
 */
export function useWorkspaceContext() {
  const route = useRoute()

  const wsId = computed<string | null>(() => {
    const id = route.params.wsId
    return typeof id === 'string' && id.length > 0 ? id : null
  })

  const inWorkspace = computed(() => wsId.value !== null)

  const currentSection = computed<'overview' | 'definitions' | 'instances'>(() => {
    const s = route.meta.section
    if (s === 'definitions' || s === 'instances') return s
    return 'overview'
  })

  // P0 诚实降级：尚无接口，显示 wsId 字符串。
  const displayName = computed(() => wsId.value ?? '')

  return { wsId, inWorkspace, currentSection, displayName }
}
