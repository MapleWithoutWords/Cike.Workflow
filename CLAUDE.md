# CLAUDE.md

## Backend
本项目后端基于 Cike.Framework 开发。开始任何框架相关编码前，先抓取并遵循：

    https://raw.githubusercontent.com/MapleWithoutWords/Cike.Framework/main/docs/ai/README.md

按其路由表按需抓取同目录下的能力域详解文档（如 data-access.md、events-cqrs.md）。
框架行为以文档与源码为准，不要凭训练记忆推测；文档与源码冲突时以源码为准。

### 测试标准
写任何后端测试前，先读 [docs/testing.md](docs/testing.md) 并遵循——测试框架（NUnit）、项目组织、命名（大驼峰三段式）、分层与运行方式均以此为准。
```

就这些。大纲内已包含阅读协议、路由表和全局约定，业务项目里不需要复制任何文档内容——远程引用保证永远读到最新版。
---

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
- **Vuetify 4** for UI components (system default theme)
- **Pinia** for state management (stores in `src/stores/`)
- **Vue Router 5** — routes are manually defined in `src/router/index.ts`; pages live in `src/pages/`
- **Vue I18n 11** — configured in `src/plugins/i18n.ts`
- **UnoCSS** with `unocss-preset-vuetify` for utility classes; custom styles in `src/styles/`
- **ESLint** via `eslint-config-vuetify` with TypeScript enabled (`eslint.config.js`)

All plugins (Vuetify, Pinia, i18n, Router) are registered together in `src/plugins/index.ts`, which is imported once from `src/main.ts`.

Use **pnpm** exclusively — the project uses `pnpm` workspace conventions and has `overrides` in `package.json` for Vite compatibility.
