# Frontend Standards

## Tech Stack

- **Location**: `Frontend/cike-workflow`
- **Package Manager**: pnpm
- **Framework**: Vue 3 (Composition API + `<script setup>`) + TypeScript + Vite
- **UI Library**: shadcn-vue (`new-york` style, `zinc` base color, lucide icons) + Tailwind CSS 4 (CSS variable theming, dark mode via `.dark` class)

## Project Structure

| Path | Responsibility |
|------|----------------|
| `src/components/ui/` | shadcn-vue generated base components (CLI-generated, see conventions below) |
| `src/components/` | Business components |
| `src/components/layout/` | App shell layout components (L1/L2 nav, topbar, breadcrumbs, theme toggle) |
| `src/composables/` | Composable functions |
| `src/layouts/` | Page layouts (`AppLayout` with sidebar / `BlankLayout` fullscreen) |
| `src/router/` | vue-router route table and configuration |
| `src/views/` | Page-level view components (one-to-one with routes) |
| `src/lib/` | Pure utilities (`cn()` etc.), no Vue dependencies |
| `src/style.css` | Tailwind entry + theme CSS variables (`:root` / `.dark`) |

Path alias: `@/` → `src/`. shadcn-vue config is in `components.json`.

## Commands

```bash
cd Frontend/cike-workflow

# Local development (Vite)
pnpm dev

# Type check + build
pnpm build

# Add shadcn-vue base components (do NOT hand-write files under src/components/ui/)
pnpm dlx shadcn-vue@latest add <component>
```
