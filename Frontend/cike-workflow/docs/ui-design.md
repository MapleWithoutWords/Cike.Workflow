# Cike.Workflow 前端 UI/UX 设计文档

> 本文档定义整个前端系统的信息架构、导航、路由、视觉规范与各页面的字段映射。
> 技术底座与编码约定以 `CLAUDE.md` 为准；本文档只在其之上补充"设计层"决策。

---

## 1. 设计原则

1. **空间是容器，不是筛选器**：一切工作流定义、实例都归属于某个工作空间；进入空间后才能看到其内部内容。
2. **两层导航，逐层下钻**：系统级只有「首页 / 空间管理」，空间内容通过卡片下钻进入，用面包屑回溯与切换。
3. **克制、高密度、专业**：管理控制台定位，浅色优先、暗色可选；信息密度偏高但保持 4/8 间距节奏。
4. **状态不靠颜色单一编码**：所有状态徽章 = 图标 + 文案 + 颜色三重表达，满足无障碍。
5. **组件靠 shadcn-vue CLI 生成**：外观底座不手改，业务层只做布局与组合（见 `CLAUDE.md` 约定）。

---

## 2. 技术底座（摘自 CLAUDE.md，此处仅登记，不重复约定）

| 项 | 取值 |
| --- | --- |
| 框架 | Vue 3 组合式 API + `<script setup>` + TypeScript + Vite |
| UI | shadcn-vue（`new-york` 风格、`zinc` 基色） |
| 样式 | Tailwind CSS 4，CSS 变量主题，暗色走 `.dark` class |
| 图标 | `lucide`（禁止 `lucide-vue-next` 或其他库） |
| 字体 | Geist Sans（正文/标题）+ Geist Mono（ID/编码/版本号等等宽场景） |
| 路径别名 | `@/` → `src/` |

---

## 3. 信息架构

```
系统
├── 首页 (Dashboard)               [系统级菜单]
│   ├── 报表区（预留）
│   ├── 新手引导
│   └── 使用文档入口
│
└── 空间管理 (Workspaces)          [系统级菜单]
    └── 空间卡片 → 点击进入空间详情
        └── 空间详情 (Workspace Detail)   [下钻，非菜单]
            ├── Tab: 工作流定义
            │   └── 目录 + 定义 混排列表（可下钻目录）
            │       └── 定义详情（版本 / 发布 / 回滚）
            └── Tab: 工作流实例
                └── 实例列表
                    └── 实例详情（活动执行记录）
```

**关键点**：
- 左侧主菜单**只有两项**：首页、空间管理。
- 空间详情**不是**左侧菜单项，而是从「空间管理」卡片下钻进入。
- 空间内部用 **Tab** 区分「工作流定义 / 工作流实例」，不再增加左侧层级。
- 目录（Folder）不是独立菜单，它与工作流定义在同一个混排列表中，可逐层下钻。

---

## 4. 路由表

| 路由 | 页面 | 说明 |
| --- | --- | --- |
| `/` | 首页 | 系统着陆页，报表 / 引导 / 文档 |
| `/workspaces` | 空间管理 | 空间卡片列表 + 新建/编辑/删除 |
| `/workspaces/:workspaceId` | 空间详情 | 默认重定向到定义 Tab |
| `/workspaces/:workspaceId/definitions` | 空间详情·定义 Tab | 目录+定义混排列表，`?folderId=` 表示当前所在目录 |
| `/workspaces/:workspaceId/definitions/:definitionId` | 定义详情 | 概览 + 版本历史 + 设计器入口（预留） |
| `/workspaces/:workspaceId/instances` | 空间详情·实例 Tab | 实例列表 + 过滤器 |
| `/workspaces/:workspaceId/instances/:instanceId` | 实例详情 | 状态 + 活动执行记录 |
| `/:pathMatch(.*)*` | 404 | 兜底 |

- 目录下钻用查询参数 `?folderId=xxx` 表达当前层级，保证深链接可分享（对应 UX「Deep Linking」）。
- Tab 切换改变的是 `definitions` / `instances` 子路由，URL 可直达具体 Tab。

---

## 5. 视觉设计系统

### 5.1 语义色板（基于 shadcn-vue zinc）

沿用 shadcn-vue `zinc` 生成的语义 token，仅额外补充**状态色**（success / warning / info），在 `src/style.css` 的 `:root` 与 `.dark` 中成对定义。业务层一律使用语义 class（`bg-primary`、`text-muted-foreground` 等），禁止硬编码色值。

| 语义 token | 用途 |
| --- | --- |
| `background` / `foreground` | 页面底色 / 主文字 |
| `card` / `card-foreground` | 卡片、表格容器 |
| `muted` / `muted-foreground` | 次级背景 / 次级文字 |
| `primary` / `primary-foreground` | 主操作按钮、选中态 |
| `secondary` / `secondary-foreground` | 次级按钮 |
| `border` / `input` / `ring` | 边框 / 输入框 / 聚焦环 |
| `destructive` | 删除、故障等破坏性操作 |
| `success` `warning` `info` | 状态徽章（新增，需在两套主题成对定义） |

> 暗色模式的边框、状态色需独立校验，不能沿用浅色值直接反相（对应「Border and divider visibility」「State contrast parity」）。

### 5.2 字体

| 场景 | 字体 | 说明 |
| --- | --- | --- |
| 标题 / 正文 | Geist Sans | 全局默认 |
| ID / Code / 版本号 / CorrelationId / 时间戳 | Geist Mono | 等宽，便于对齐与识别 |

字号节奏（Tailwind 默认 scale）：页面标题 `text-2xl`、区块标题 `text-lg`、正文 `text-sm`、辅助 `text-xs`。

### 5.3 间距、圆角、阴影

- 间距遵循 4/8 节奏：组件内 `gap-2`/`gap-3`，区块间 `space-y-6`，页面内边距 `p-6`。
- 圆角、阴影、边框一律使用 shadcn-vue 组件自带底座，不额外覆盖。
- 内容区最大宽度：列表/表格页全宽（`w-full`），详情表单页限制 `max-w-3xl` 保证可读行宽。

### 5.4 图标（lucide）

| 语义 | 图标 | 备注 |
| --- | --- | --- |
| 首页 | `LayoutDashboard` | 侧栏 |
| 空间管理 | `Boxes` | 侧栏 |
| 目录 | `Folder` / `FolderOpen` | 混排列表行首 |
| 工作流定义 | `Workflow` | 混排列表行首 / Tab |
| 实例 | `Activity` | Tab |
| 新建 | `Plus` | 主按钮 |
| 编辑 | `Pencil` | 行操作 |
| 删除 | `Trash2` | 行操作，destructive |
| 移动 | `FolderInput` | 目录/定义移动 |
| 发布 | `Upload` | 定义详情 |
| 回滚 | `History` | 版本历史 |
| 搜索 | `Search` | 过滤栏 |
| 主题切换 | `Sun` / `Moon` | 顶栏 |
| 面包屑分隔 | `ChevronRight` | — |

- 装饰性图标（旁边已有可见文字）加 `aria-hidden="true"`。
- 纯图标按钮（如行内编辑/删除）必须提供 `aria-label` 无障碍名。

---

## 6. 状态 / 类型映射（来自后端枚举）

### 6.1 工作流实例状态 `WorkflowStatus`

| 枚举 | 中文 | 徽章色 | lucide 图标 |
| --- | --- | --- | --- |
| `Pending` | 等待中 | muted | `Clock` |
| `Executing` | 执行中 | info | `Loader`（可旋转） |
| `Suspended` | 已挂起 | warning | `PauseCircle` |
| `Finished` | 已完成 | success | `CheckCircle2` |
| `Cancelled` | 已取消 | muted | `XCircle` |
| `Faulted` | 已故障 | destructive | `AlertTriangle` |
| `Interrupted` | 已中断 | warning | `Ban` |

> 每个徽章 = 图标 + 文案 + 颜色，禁止只用颜色区分（对应「Color Only」High 级）。
> `IsExecuting=true` 时状态列可叠加轻微动效，但需遵守 `prefers-reduced-motion`。
> `IncidentCount > 0` 在实例行额外显示红色计数徽章（`role="status"`，读屏播报"N 个异常"）。

### 6.2 活动执行状态 `ActivityStatus`（实例详情用）

| 枚举 | 中文 | 徽章色 |
| --- | --- | --- |
| `Pending` | 等待 | muted |
| `Running` | 运行中 | info |
| `Completed` | 已完成 | success |
| `Canceled` | 已取消 | muted |
| `Faulted` | 故障 | destructive |

### 6.3 工作流定义类型 `WorkflowDefinitionType`

| 枚举值 | 中文 | 徽章 |
| --- | --- | --- |
| `Workflow (1)` | 工作流 | secondary |
| `AgentWorkflow (2)` | 智能体工作流 | info |
| `Approval (3)` | 审批流 | warning |

---

## 7. 全局布局（AppShell）

```
┌──────────┬──────────────────────────────────────────────────┐
│ Logo     │  顶栏：面包屑 ……………………………… [主题] [用户]     │
│          ├──────────────────────────────────────────────────┤
│ 首页      │                                                  │
│ 空间管理  │              路由内容区 <RouterView/>             │
│          │                                                  │
│ (collapse)│                                                 │
└──────────┴──────────────────────────────────────────────────┘
```

- **左侧栏**：Logo + 两个菜单项（首页、空间管理），底部折叠按钮。选中项高亮（`bg-accent` + 左侧色条），对应 UX「Active State」。
- **顶栏**：左侧面包屑，右侧主题切换 + 用户区。sticky 固定，内容区加 `pt` 补偿避免遮挡（对应「Sticky Navigation」）。
- **面包屑**：反映当前层级且**可点击回跳**。空间层级节点支持下拉切换到同级其他空间（对应用户诉求"面包屑支持切换"）。

### 面包屑层级示例

| 当前页 | 面包屑 |
| --- | --- |
| 空间管理 | 首页 › 空间管理 |
| 空间详情·定义 | 首页 › 空间管理 › **财务系统 ▾** › 工作流定义 |
| 目录下钻 | 首页 › 空间管理 › 财务系统 › 工作流定义 › 报销流程 |
| 定义详情 | 首页 › 空间管理 › 财务系统 › 工作流定义 › 月度报销审批 |
| 实例详情 | 首页 › 空间管理 › 财务系统 › 工作流实例 › #实例名 |

> `财务系统 ▾` 为可切换节点：点击弹出空间列表 Popover，选中即跳转到目标空间的对应 Tab。

---

## 8. 页面设计与字段映射

### 8.1 首页 `/`

- **目的**：系统着陆页；当前阶段以占位为主，预留扩展。
- **结构**：
  - 顶部欢迎语 + 快捷入口卡片（「进入空间管理」）。
  - 报表区：占位卡片（图表待接入，标注"即将上线"，不放空白），对应「Empty States」。
  - 新手引导卡片：分步骤引导（创建空间 → 新建目录 → 新建定义）。
  - 使用文档：外链/内嵌文档入口卡片。
- **DTO**：无（纯前端着陆页）。

### 8.2 空间管理 `/workspaces`

- **布局**：顶部标题栏（标题 + 搜索框 + `新建空间` 主按钮）+ 卡片网格（响应式 1/2/3 列）。
- **卡片字段**（`WorkspaceItemDto`）：

  | 展示 | 字段 |
  | --- | --- |
  | 卡片标题 | `name` |
  | 编码徽章（Mono） | `code` |
  | 描述（截断 2 行） | `description` |
  | 页脚辅助文字 | `lastModificationTime`（相对时间）/ `creationTime` |

- **卡片交互**：整卡点击 → 进入空间详情；右上角 `⋯` 菜单 = 编辑 / 删除。
- **空状态**：无空间时展示引导插画 + `新建空间` 按钮（「Empty States」）。

#### 新建空间 `WorkspaceForm.vue`（`AddWorkspaceDto`）

| 字段 | 控件 | 规则 |
| --- | --- | --- |
| `code?` | Input（Mono） | 可选；留空由后端生成；建议校验唯一性提示 |
| `name` | Input | 必填 |
| `description` | Textarea | 可选，默认空串 |

#### 编辑空间（`UpdateWorkspaceDto`）

| 字段 | 控件 | 规则 |
| --- | --- | --- |
| `name` | Input | 必填 |
| `description` | Textarea | 可选 |

> `code` 编辑态只读展示（后端更新 DTO 不含 code）。
> 删除用 `AlertDialog` 二次确认。

### 8.3 空间详情 · 工作流定义 Tab `/workspaces/:id/definitions`

- **布局**：Tab 头（工作流定义 / 工作流实例）+ 操作栏（面包屑目录路径 + 搜索 + `新建目录` + `新建定义`）+ 混排列表（表格）。
- **混排列表**（`WorkflowDefinitionFolderItemDto`，多态 `type` 区分）：

  行首图标按 `type` 区分：`Folder(1)` → 文件夹图标；`WorkflowDefinition(2)` → 工作流图标。

  **目录行**（`FolderItemDto`）：

  | 列 | 字段 |
  | --- | --- |
  | 名称 | `name`（点击进入该目录，更新 `?folderId`） |
  | 类型 | 「目录」 |
  | 操作 | 重命名 / 移动 / 删除 |

  **定义行**（`WorkflowDefinitionItemDto`）：

  | 列 | 字段 | 说明 |
  | --- | --- | --- |
  | 名称 | `name` | 点击进入定义详情 |
  | 类型 | `type` | 徽章（见 6.3） |
  | 版本 | `version` + `isLatest` | Mono，`vN`；最新版加"最新"徽章 |
  | 发布状态 | `publishedVersion` | 有值→"已发布 vN"success；无→"未发布"muted |
  | 标记 | `isReadonly` / `isSystem` / `usableAsActivity` | 只读/系统/可作为活动 → 小徽章 |
  | 修改时间 | `lastModificationTime` | 相对时间 |
  | 操作 | 编辑 / 移动 / 删除 | 系统或只读项禁用编辑/删除并说明 |

- **列表规范**：
  - 表格外层 `overflow-x-auto`，窄屏可横向滚动（「Table Handling」）。
  - 加载态用稳定骨架屏 + `aria-busy`，避免闪烁（「Loading Indicators」）。
  - 面包屑目录路径来自 `path[]`（`FolderPathDto`）+ 顶层"根目录"。

#### 新建目录（`AddFolderDto`）

| 字段 | 控件 | 规则 |
| --- | --- | --- |
| `workspaceId` | 隐藏 | 取自路由 |
| `parentId` | 隐藏 | 当前 `folderId`，根目录传 0 |
| `name` | Input | 必填 |

#### 重命名目录（`UpdateFolderDto`）：仅 `name`。
#### 移动目录（`MoveFolderDto`）：`parentId` 目录选择器，0=根目录。

#### 新建定义 `WorkflowDefinitionForm.vue`（`AddWorkflowDefinitionDto`）

| 字段 | 控件 | 规则 |
| --- | --- | --- |
| `workspaceId` | 隐藏 | 路由 |
| `folderId` | 隐藏 | 当前目录，根目录 0 |
| `definitionId?` | Input(Mono) | 可选，留空由后端生成 |
| `name` | Input | 必填 |
| `description` | Textarea | 可选 |
| `type` | Select | 工作流 / 智能体工作流 / 审批流 |
| `usableAsActivity` | Switch | 是否可作为活动被复用 |

#### 移动定义（`MoveWorkflowDefinitionDto`）：`folderId` 目录选择器，0=根目录。

### 8.4 定义详情 `/workspaces/:id/definitions/:definitionId`

- **布局**：详情头（名称 + 类型徽章 + 版本选择器 + 操作按钮）+ 分区内容。
- **头部字段**（`WorkflowDefinitionDetailDto`）：

  | 展示 | 字段 |
  | --- | --- |
  | 标题 | `name` |
  | 编码 | `definitionId`（Mono） |
  | 类型 | `type` 徽章 |
  | 版本选择器 | `version` + `isLatest` + `isPublished` |
  | 只读/系统标记 | `isReadonly` / `isSystem` |

- **概览区**：`description`、`materializerName`、`usableAsActivity`、`folderId`（显示所属目录路径）、`options`（键值展示）。`root`（活动树）当前阶段折叠为只读 JSON / 占位（可视化设计器预留）。
- **版本历史区**（`WorkflowDefinitionVersionItemDto` 列表）：

  | 列 | 字段 |
  | --- | --- |
  | 版本 | `version`（Mono）+ `isLatest` / `isPublished` 徽章 |
  | 发布备注 | `publishedNote` |
  | 发布人 | `publishedBy` |
  | 发布时间 | `publishedAt` |
  | 操作 | 回滚到此版本 |

- **操作**：
  - 发布（`PublishWorkflowDefinitionDto`）：`publishedNote?` 备注输入的对话框。
  - 回滚（`RollbackWorkflowDefinitionDto`）：`definitionId` + `definitionVersionId`，`AlertDialog` 确认。
  - 编辑（`UpdateWorkflowDefinitionDto`）：`name` / `description` / `type` / `usableAsActivity`。
  - 保存设计（`SaveWorkflowDefinitionDto`：`root` + `options`）→ 设计器阶段接入，当前预留。
  - `isReadonly` / `isSystem` 为真时，编辑/发布/保存按钮禁用并 tooltip 说明原因（「Disabled state clarity」）。

### 8.5 空间详情 · 工作流实例 Tab `/workspaces/:id/instances`

- **布局**：Tab 头 + 过滤栏（搜索 + 状态多选 + 时间范围）+ 实例表格（只读监控）。
- **表格字段**（`WorkflowInstanceItemDto`）：

  | 列 | 字段 | 说明 |
  | --- | --- | --- |
  | 状态 | `Status` | 徽章（见 6.1） |
  | 名称 | `Name` | 空则回退显示 `DefinitionName` |
  | 定义 | `DefinitionName` + `Version` | 链接到对应定义详情 |
  | 关联 ID | `CorrelationId` | Mono，可复制 |
  | 异常 | `IncidentCount` | >0 红色计数徽章 |
  | 执行中 | `IsExecuting` | 执行中时状态列动效提示 |
  | 完成时间 | `FinishedAt` | 未完成显示"—" |
  | 父实例 | `ParentWorkflowInstanceId` | >0 时可跳转父实例 |
  | 操作 | 查看详情 | — |

- 行点击进入实例详情。过滤器状态改变同步到 URL 查询参数（深链接）。

### 8.6 实例详情 `/workspaces/:id/instances/:instanceId`

- **头部字段**（`WorkflowInstanceDetailDto` 继承 `WorkflowInstanceItemDto`）：状态徽章 + `Name` + `DefinitionName`(链接) + `Version` + `CorrelationId`(Mono) + `FinishedAt`。
- **工作流状态区**：`WorkflowState`（键值 / 只读 JSON 展示）。
- **活动执行记录**（`ActivityInstanceExecutionRecordDto` 列表/时间线）：

  | 列 | 字段 |
  | --- | --- |
  | 活动名 | `ActivityName`（空则 `ActivityType`） |
  | 类型 | `ActivityType` + `ActivityTypeVersion` |
  | 状态 | `Status`（活动状态徽章，见 6.2） |
  | 异常 | `Exception`（有则展开错误详情）+ `AggregateFaultCount` |
  | 完成时间 | `FinishedAt` |
  | 调用深度 | `CallStackDepth`（用于缩进表达调用层级） |

- `ActivityState` / `Payload` / `Outputs` / `Properties` / `Metadata`：行展开后以只读键值/JSON 面板呈现。
- `HasBookmarks`：有书签的活动加标记图标。

---

## 9. 组件清单

### 9.1 需通过 shadcn-vue CLI 生成的基础组件

`sidebar`、`breadcrumb`、`button`、`card`、`table`、`tabs`、`badge`、`dialog`、`alert-dialog`、`dropdown-menu`、`input`、`textarea`、`select`、`switch`、`popover`、`skeleton`、`sonner`（Toast）、`tooltip`、`separator`。

### 9.2 业务组件（`src/components/`）

| 组件 | 职责 |
| --- | --- |
| `layout/AppSidebar.vue` | 左侧两项主菜单 + 折叠 |
| `layout/AppHeader.vue` | 顶栏：面包屑 + 主题 + 用户 |
| `layout/AppBreadcrumb.vue` | 面包屑，含空间可切换节点 |
| `layout/ThemeToggle.vue` | 明暗主题切换 |
| `workspace/WorkspaceCard.vue` | 空间卡片 |
| `workspace/WorkspaceForm.vue` | 新建/编辑空间表单 |
| `definition/DefinitionFolderTable.vue` | 目录+定义混排列表 |
| `definition/WorkflowDefinitionForm.vue` | 新建/编辑定义表单 |
| `definition/FolderForm.vue` | 新建/重命名目录 |
| `definition/MoveTargetDialog.vue` | 目录/定义移动目标选择 |
| `definition/VersionHistoryTable.vue` | 版本历史 + 回滚 |
| `definition/PublishDialog.vue` | 发布备注对话框 |
| `instance/InstanceTable.vue` | 实例列表 + 过滤 |
| `instance/InstanceStatusBadge.vue` | 实例状态徽章 |
| `instance/ActivityRecordTimeline.vue` | 活动执行记录 |
| `common/StatusBadge.vue` | 通用状态徽章（图标+文案+色） |
| `common/EmptyState.vue` | 空状态 |
| `common/PageHeader.vue` | 页面标题 + 操作区 |

> 表单统一 `*Form.vue`，对话框 `*Dialog.vue`，确认框用 `AlertDialog`（见 CLAUDE.md 命名约定）。

---

## 10. 交互与无障碍规范（Web 桌面）

- **键盘可达**：所有可操作元素可 Tab 到达，聚焦可见（`ring`），Tab 顺序与视觉一致。
- **焦点管理**：路由切换后焦点移至主内容区 `#main-content`；对话框打开锁定焦点、关闭归还触发元素。
- **状态不靠颜色**：徽章、异常计数均带图标/文字。
- **表单反馈**：提交经历 加载 → 成功/失败；字段级错误内联保留，多错误在提交后聚焦错误摘要。
- **动效**：过渡 150–300ms；执行中动效、骨架屏均遵守 `prefers-reduced-motion`。
- **实时播报**：实例状态/异常计数变化用 `role="status"` 原子播报，不抢焦点。
- **表格**：外层横向滚动兜底；提供多选 + 批量操作的扩展位（当前实例监控为只读，可后置）。

---

## 11. 响应式断点

| 断点 | 布局行为 |
| --- | --- |
| `< 768px` | 侧栏收起为抽屉；空间卡片 1 列；表格横向滚动 |
| `768–1024px` | 侧栏可折叠为图标；卡片 2 列 |
| `> 1024px` | 侧栏展开；卡片 3 列；表格全宽 |

内容区随断点调整水平内边距（`px-4` → `px-6` → `px-8`）。

---

## 12. 阶段边界（与既有 PRD 一致）

- **本阶段包含**：空间/目录/定义的管理 CRUD、定义版本与发布/回滚、实例只读监控。
- **本阶段预留**：可视化流程设计器（`root` 活动树编辑、`SaveWorkflowDefinitionDto`）、首页报表图表接入。
- **数据层**：类型来自后端 OpenAPI 生成（`src/api/generated`，见 `openapi-ts.config.ts`），long 序列化为 string。
