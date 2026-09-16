<script setup lang="ts">
import { ref, onMounted } from "vue"
import { RouterLink } from "vue-router"
import { Plus, Search, Boxes, Pencil, Trash2 } from "@lucide/vue"
import { getApiV1WorkspacesPagedList, deleteApiV1Workspaces } from "@/api"
import type { WorkspaceItemDto } from "@/api"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import WorkspaceFormDialog from "@/components/WorkspaceFormDialog.vue"

// --- 列表数据 ---
const workspaces = ref<WorkspaceItemDto[]>([])
const loading = ref(false)
const loadError = ref(false)
const total = ref(0)
const keyword = ref("")
const page = ref(1)
const pageSize = ref(12)

async function fetchWorkspaces() {
  loading.value = true
  loadError.value = false
  try {
    const { data, error } = await getApiV1WorkspacesPagedList({
      query: {
        keyword: keyword.value || undefined,
        Page: page.value,
        PageSize: pageSize.value,
      },
    })
    if (error) {
      loadError.value = true
      return
    }
    workspaces.value = data?.items ?? []
    total.value = Number(data?.total ?? 0)
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  page.value = 1
  fetchWorkspaces()
}

// --- 新建 / 编辑 ---
const formDialogOpen = ref(false)
const editingWorkspace = ref<WorkspaceItemDto | null>(null)

function openCreate() {
  editingWorkspace.value = null
  formDialogOpen.value = true
}

function openEdit(ws: WorkspaceItemDto) {
  editingWorkspace.value = ws
  formDialogOpen.value = true
}

function onSaved() {
  fetchWorkspaces()
}

// --- 删除 ---
const deleteDialogOpen = ref(false)
const deletingWorkspace = ref<WorkspaceItemDto | null>(null)
const deleting = ref(false)

function openDelete(ws: WorkspaceItemDto) {
  deletingWorkspace.value = ws
  deleteDialogOpen.value = true
}

async function confirmDelete() {
  if (!deletingWorkspace.value) return
  deleting.value = true
  try {
    const { error } = await deleteApiV1Workspaces({
      query: { workspaceId: deletingWorkspace.value.id! },
    })
    if (error) return
    deleteDialogOpen.value = false
    deletingWorkspace.value = null
    fetchWorkspaces()
  } finally {
    deleting.value = false
  }
}

onMounted(fetchWorkspaces)
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold tracking-tight">空间管理</h1>
        <p class="mt-1 text-sm text-muted-foreground">
          工作空间用于划分工作流的管理范围
        </p>
      </div>
      <Button @click="openCreate">
        <Plus />
        新建空间
      </Button>
    </div>

    <!-- Search -->
    <div class="relative max-w-sm">
      <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
      <Input
        v-model="keyword"
        placeholder="搜索空间..."
        class="pl-9"
        @keyup.enter="handleSearch"
      />
    </div>

    <!-- Workspace Cards -->
    <div v-if="loading" class="text-sm text-muted-foreground">加载中...</div>

    <div v-else-if="workspaces.length" class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="ws in workspaces"
        :key="ws.id"
        class="group relative rounded-lg border p-5 transition-colors hover:border-primary/50 hover:bg-accent/30"
      >
        <!-- 操作按钮（右下角，避免与 code 徽章重叠） -->
        <div class="absolute bottom-3 right-3 flex gap-1 opacity-0 transition-opacity group-hover:opacity-100">
          <Button variant="ghost" size="icon-xs" title="编辑" @click.prevent="openEdit(ws)">
            <Pencil />
          </Button>
          <Button variant="ghost" size="icon-xs" title="删除" class="text-destructive hover:text-destructive" @click.prevent="openDelete(ws)">
            <Trash2 />
          </Button>
        </div>

        <RouterLink :to="`/workspaces/${ws.id}`" class="block">
          <div class="flex items-start justify-between">
            <div class="flex h-10 w-10 items-center justify-center rounded-md bg-primary/10 text-primary">
              <Boxes :size="20" />
            </div>
            <span class="rounded bg-muted px-2 py-0.5 font-mono text-xs text-muted-foreground">
              {{ ws.code }}
            </span>
          </div>
          <h3 class="mt-3 font-medium">{{ ws.name }}</h3>
          <p class="mt-1 line-clamp-2 text-sm text-muted-foreground">
            {{ ws.description }}
          </p>
          <p class="mt-3 text-xs text-muted-foreground">
            更新于 {{ ws.updatedAt }}
          </p>
        </RouterLink>
      </div>
    </div>

    <!-- 加载失败 -->
    <div v-else-if="loadError" class="flex flex-col items-center justify-center rounded-lg border border-dashed py-16">
      <h3 class="font-medium">加载失败</h3>
      <p class="mt-1 text-sm text-muted-foreground">
        无法获取空间列表，请检查后端服务后重试
      </p>
      <Button class="mt-4" variant="outline" @click="fetchWorkspaces">
        重试
      </Button>
    </div>

    <!-- Empty State -->
    <div v-else class="flex flex-col items-center justify-center rounded-lg border border-dashed py-16">
      <Boxes :size="40" class="text-muted-foreground/50" />
      <h3 class="mt-4 font-medium">暂无工作空间</h3>
      <p class="mt-1 text-sm text-muted-foreground">
        创建一个工作空间来开始管理你的工作流
      </p>
      <Button class="mt-4" @click="openCreate">
        <Plus />
        新建空间
      </Button>
    </div>

    <!-- 新建 / 编辑对话框 -->
    <WorkspaceFormDialog
      v-model:open="formDialogOpen"
      :workspace="editingWorkspace"
      @saved="onSaved"
    />

    <!-- 删除确认对话框 -->
    <Dialog v-model:open="deleteDialogOpen">
      <DialogContent class="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>确认删除</DialogTitle>
          <DialogDescription>
            确定要删除空间「{{ deletingWorkspace?.name }}」吗？此操作不可撤销。
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" @click="deleteDialogOpen = false">
            取消
          </Button>
          <Button variant="destructive" :disabled="deleting" @click="confirmDelete">
            {{ deleting ? '删除中...' : '删除' }}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  </div>
</template>
