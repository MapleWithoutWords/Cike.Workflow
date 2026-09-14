<script setup lang="ts">
import { useMediaQuery } from '@vueuse/core'
import { computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from '@/components/ui/sidebar'
import { useWorkspaceContext } from '@/composables/useWorkspaceContext'
import { L1_NAV, L2_NAV, isL1Active } from './navigation'

const route = useRoute()
const { wsId, inWorkspace, currentSection, displayName } = useWorkspaceContext()
const isMobile = useMediaQuery('(max-width: 1023px)')

/**
 * L2 渲染条件：
 * - 空间上下文内：常驻 L2
 * - <1024：即使不在空间内也要渲染，作为 L1+L2 合并抽屉的载体（MASTER §7）
 */
const visible = computed(() => isMobile.value || inWorkspace.value)

function l2To(section: string) {
  const id = wsId.value ?? ''
  if (section === 'definitions') return { name: 'definitions', params: { wsId: id } }
  if (section === 'instances') return { name: 'instances', params: { wsId: id } }
  return { name: 'workspace-detail', params: { wsId: id } }
}
</script>

<template>
  <Sidebar v-if="visible" collapsible="icon">
    <!--
      空间上下文标识（只读）。空间的选择入口是 /workspaces 列表页，
      这里不做下拉切换器。
    -->
    <SidebarHeader v-if="inWorkspace" class="gap-1">
      <div class="flex flex-col gap-0.5 px-2 py-1 group-data-[collapsible=icon]:hidden">
        <span class="text-[11px] uppercase tracking-wide text-muted-foreground">当前空间</span>
        <span class="truncate font-mono text-xs font-medium">{{ displayName }}</span>
      </div>
    </SidebarHeader>

    <SidebarContent>
      <!-- <1024：L1+L2 合并为单抽屉（MASTER §7） -->
      <SidebarGroup v-if="isMobile">
        <SidebarGroupLabel>全局</SidebarGroupLabel>
        <SidebarGroupContent>
          <SidebarMenu>
            <SidebarMenuItem v-for="item in L1_NAV" :key="item.key">
              <SidebarMenuButton
                :as="RouterLink"
                :to="item.to"
                :is-active="isL1Active(item, route.name as string | undefined, route.path)"
                :aria-current="isL1Active(item, route.name as string | undefined, route.path) ? 'page' : undefined"
                :tooltip="item.label"
              >
                <component :is="item.icon" />
                <span>{{ item.label }}</span>
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarGroupContent>
      </SidebarGroup>

      <!-- L2 空间功能区 -->
      <SidebarGroup v-if="inWorkspace">
        <SidebarGroupLabel>空间</SidebarGroupLabel>
        <SidebarGroupContent>
          <SidebarMenu>
            <SidebarMenuItem v-for="item in L2_NAV" :key="item.section">
              <SidebarMenuButton
                :as="RouterLink"
                :to="l2To(item.section)"
                :is-active="currentSection === item.section"
                :aria-current="currentSection === item.section ? 'page' : undefined"
                :tooltip="item.label"
              >
                <component :is="item.icon" />
                <span>{{ item.label }}</span>
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarGroupContent>
      </SidebarGroup>
    </SidebarContent>

    <SidebarRail />
  </Sidebar>
</template>
