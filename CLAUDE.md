# CLAUDE.md

## 项目概览
Cike.Workflow：基于 Cike.Framework 的工作流引擎。后端 .NET 分层单模（`Backend/Cike.Workflow.sln`），前端 Vue 3 + Vuetify 4 管理台。

| 路径 | 职责 |
|---|---|
| `Backend/src/Cike.Workflow.Core` | 工作流核心模型与执行（活动、校验器、序列化） |
| `Backend/src/Cike.Workflow.Domain`（`.Domain.Shared`） | 领域实体 / 共享常量与枚举 |
| `Backend/src/Cike.Workflow.Application`（`.Application.Contracts`） | 应用服务 / DTO 与接口定义 |
| `Backend/src/Cike.Workflow.EntityFrameworkCore` | DbContext、仓储、EF 迁移 |
| `Backend/src/Cike.Workflow.Service.Open` | 宿主（Program.cs、HTTP 入口） |
| `Backend/src/Cike.Workflow.{Caching,Common,Expressions}` | 缓存 / 序列化与公共组件 / 表达式求值 |
| `Backend/tests/*` | Core.Tests / EntityFrameworkCore.Tests / Service.Open.Tests（含 HTTP 集成测试） |
| `Frontend/cike-workflow` | Vue 3 管理台 |

## Backend
本项目后端基于 Cike.Framework 开发。开始任何框架相关编码前，先抓取并遵循：

    https://raw.githubusercontent.com/MapleWithoutWords/Cike.Framework/main/docs/ai/README.md

按其路由表按需抓取同目录下的能力域详解文档（如 data-access.md、events-cqrs.md）。
框架行为以文档与源码为准，不要凭训练记忆推测；文档与源码冲突时以源码为准。

### Commands

```bash
# 全量测试（任务收尾 / 提交前跑）
dotnet test Backend/Cike.Workflow.sln

# 快速反馈：仅单元测试（排除集成测试）
dotnet test Backend/Cike.Workflow.sln --filter "Category!=Integration"

# 定向调试单个测试项目
dotnet test Backend/tests/Cike.Workflow.Core.Tests

# 新增 EF 迁移
dotnet ef migrations add <Name> --project Backend/src/Cike.Workflow.EntityFrameworkCore --startup-project Backend/src/Cike.Workflow.Service.Open
```

## Frontend

### Commands

```bash
cd Frontend/cike-workflow

# Install dependencies
pnpm install

# Start dev server
pnpm dev

# Type check + production build (runs both in parallel)
pnpm build

# Type check only
pnpm type-check

# Lint
pnpm lint

# Lint and auto-fix
pnpm lint:fix

# Apply Ruler MCP config (Vuetify MCP integration)
pnpm mcp

# Revert Ruler MCP config
pnpm mcp:revert
```

### Architecture

- **Vue 3** with Composition API and `<script setup>`
- **Vuetify 4** for UI components (Catppuccin light/dark themes, see 主题配色)
- **Pinia** for state management (stores in `src/stores/`)
- **Vue Router 5** — routes are manually defined in `src/router/index.ts`; pages live in `src/pages/`
- **Vue I18n 11** — configured in `src/plugins/i18n.ts`
- **UnoCSS** with `unocss-preset-vuetify` for utility classes; custom styles in `src/styles/`
- **ESLint** via `eslint-config-vuetify` with TypeScript enabled (`eslint.config.js`)

All plugins (Vuetify, Pinia, i18n, Router) are registered together in `src/plugins/index.ts`, which is imported once from `src/main.ts`.

Use **pnpm** exclusively — the project uses `pnpm` workspace conventions and has `overrides` in `package.json` for Vite compatibility.

### 主题配色（Catppuccin）

设计令牌唯一来源：`src/plugins/catppuccin.ts`（色值取自官方 [catppuccin/palette](https://github.com/catppuccin/palette) v1.8.0）。暗色 = Mocha，浅色 = Latte，主 accent = mauve；两套主题同源，随 `app.theme-preference`（localStorage）或系统偏好切换。

**角色映射**（Mocha 值 / Latte 值）：

| 用途 | 取色 | Mocha | Latte |
|---|---|---|---|
| 主 accent / 智能体工作流 | `primary` / mauve | #CBA6F7 | #8839EF |
| 页面底色 | `background` / base | #1E1E2E | #EFF1F5 |
| 卡片面板 | `surface` | #313244 | #FFFFFF |
| 悬停/次级面板 | `surface-variant` | #45475A | #CCD0DA |
| 次要文字 | `muted` | #A6ADC8 | #6C6F85 |
| 边框/分隔 | `overlay` | #6C7086 | #9CA0B0 |
| 最深层（弹层底） | `crust` | #11111B | #DCE0E8 |

**业务语义固定映射**——工作流类型徽标：普通工作流 = teal、智能体工作流 = mauve、审批流 = peach；实例状态：运行中 = sky、成功 = green（= success）、失败 = red（= error）、已取消 = muted。这套语义色同时是"定义颜色身份"的分配池。

**使用规则：**

- 禁止在业务代码里硬编码 hex；用组件 `color` prop（`<v-chip color="teal">`，14 个功能色均已注册为 Vuetify 主题色）或 CSS 变量 `rgb(var(--v-theme-<name>))`
- UnoCSS 颜色工具类（`bg-primary` / `text-teal` 等）基于上述变量生成，亮暗切换自动跟色；`dark:` / `light:` 前缀变体可用
- 主题偏好读写走 `useAppTheme()`（`src/composables/useAppTheme.ts`），不要直接操作 Vuetify theme 实例或 localStorage

### 约定

- **目录**：`pages/` 放路由页；`components/` 放可复用组件，PascalCase 多词命名（如 `WorkflowCard.vue`）；后端接口调用统一封装在 `src/api/`
- **组件**：一律 `<script setup lang="ts">`；props / emits 用类型式声明（`defineProps<{ ... }>()`），不用运行时对象式
- **状态**：跨页共享状态进 `stores/`（`useXxxStore` 命名）；仅单页用的状态留在组件内，不建 store
- **i18n**：默认语言中文；所有用户可见文案走 `t()`，不硬编码字符串；语言包用独立文件，不内联在 `plugins/i18n.ts`
- **类型**：仅导入类型时用 `import type { X }`；禁止 `as any`
- **验证**：迭代中跑 `pnpm type-check` 即可，`pnpm build` 留到任务收尾——不要每改一次就全量 build

## 约定

- **提交信息**：Conventional Commits（feat / fix / test / docs / refactor）+ 中文描述，如 `feat:工作流定义保存/发布全链路`
- **语言**：与用户交流、提交信息、`.claude/rules/` 与项目文档用中文；代码标识符与注释用英文
- **变更体积**：非机械改动预计超过 ~500 行时，先提出拆分方案再动手
- **分层纪律**：业务逻辑不进 `Service.Open`——宿主只做组装与入口
- **自动格式化**：代码改完后自动格式化——后端对改动的项目跑 `dotnet format <项目>.csproj`，前端跑 `pnpm lint:fix`——无需请求批准；但只提交自己改动文件的格式化结果，不把无关文件扫进同一提交
- **测试随行**：改动工作流执行、序列化、仓储等核心逻辑时，必须同步新增或更新测试（写法遵循 `.claude/rules/testing.md`，核心行为优先集成测试）；修 bug 先写复现测试再修
- **文件大小**：新功能优先放新文件；单文件超过 ~800 行时不再往里堆，除非有明确理由

## 禁止事项

- **不反编译**：不使用 dnSpy / ILSpy / dotPeek / ildasm 等工具反编译 DLL 或其他二进制文件。遇到不懂的框架/库行为，按顺序处理：① 读源码与官方文档 → ② 网络搜索 → ③ 向用户提问。不允许凭训练记忆猜测框架行为。
- **不手改生成文件**：EF Core 迁移（`*ModelSnapshot.cs`、`*.Designer.cs`）只能由 `dotnet ef` 命令生成；确需手改必须先征得用户同意。
- **不擅自引入依赖**：新增 NuGet 包或 pnpm 包之前必须询问用户。
- **不破坏测试**：不允许为了让测试通过而修改断言、删除或跳过测试。
- **不扩大范围**：只改任务要求的文件，不顺手重构、重命名或清理无关代码。
- **不隐藏问题**：测试失败、验证不通过、无法确认的行为必须如实报告，不允许含糊带过或声称完成。
