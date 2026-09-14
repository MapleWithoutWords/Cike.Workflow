import type { Component } from 'vue'
import type { RouteLocationRaw } from 'vue-router'
import { Activity, House, Info, LayoutGrid, Workflow } from '@lucide/vue'

export interface NavL1Item {
  key: 'home' | 'workspaces'
  label: string
  icon: Component
  /** 用于判断激活态：精确匹配或前缀匹配 */
  match: (name: string | undefined, path: string) => boolean
  to: RouteLocationRaw
}

export interface NavL2Item {
  section: 'overview' | 'definitions' | 'instances'
  label: string
  icon: Component
}

/** L1 全局侧栏：常驻，Logo + 两项（MASTER §4） */
export const L1_NAV: NavL1Item[] = [
  {
    key: 'home',
    label: '首页',
    icon: House,
    to: { name: 'home' },
    match: (name) => name === 'home',
  },
  {
    key: 'workspaces',
    label: '工作空间',
    icon: LayoutGrid,
    to: { name: 'workspaces' },
    match: (name, path) =>
      name === 'workspaces' || path.startsWith('/w/'),
  },
]

/** L2 空间侧栏：进入某空间后展开（MASTER §4） */
export const L2_NAV: NavL2Item[] = [
  { section: 'overview', label: '空间详情', icon: Info },
  { section: 'definitions', label: '工作流定义', icon: Workflow },
  { section: 'instances', label: '工作流实例', icon: Activity },
]

/** 根据当前路由名判断 L1 项是否激活 */
export function isL1Active(item: NavL1Item, routeName: string | undefined, routePath: string): boolean {
  return item.match(routeName, routePath)
}
