# Coding Conventions

## Component Rules

- **Base components via CLI only**: Components under `src/components/ui/` must be added via `shadcn-vue add`, never hand-written from scratch. CLI config is in `components.json`.
- **ui/ vs business component separation**: Business components go in `src/components/`, PascalCase naming, filename matches component name. Customizations to `ui/` components that are reused across multiple places should be wrapped in business wrapper components, not patched directly in `ui/` source files.
- **Component naming patterns**: Form components are separate as `*Form.vue`, dialog components as `*Dialog.vue` / `*Modal.vue` (confirmation dialogs use `AlertDialog`), never inlined in pages or other components.

## Style Rules

- **Business styles handle layout & composition only** (spacing, arrangement, sizing), do NOT modify `ui/` component appearance base (border-radius / border / shadow / color scheme).
- **Colors must use theme variables** (`bg-primary`, `text-muted-foreground`, etc.), NO hardcoded color values, NO tokens not defined in `src/style.css`.
- **Multi-class merging**: Use `cn()` from `@/lib/utils`.

## Icons

- **Unified icon library**: Use `@lucide/vue` only, NO `lucide-vue-next` or other icon libraries.

## Workflow Designer

- **Graph editing engine**: AntV X6 3.x (`@antv/x6` + `@antv/x6-vue-shape`) only. Vue SFCs register as node shapes via `x6-vue-shape`'s `register()`.
- **Official plugins**: Import as needed (3.x exports `History` / `Clipboard` / `Keyboard` / `Selection` / `MiniMap` / `Snapline` / `Stencil` / `Dnd` / `Export` / `Scroller` / `Transform` from main package, no need for `@antv/x6-plugin-*` sub-packages).
- **NO alternative graph libraries**: Do not introduce vue-flow / react-flow / bpmn-js / LogicFlow or similar.

## Verification

- **After changes**: Run `pnpm build` (includes vue-tsc type checking) to confirm no type errors.

## Current Infrastructure Boundaries

- Router is implemented (vue-router 4, route table in `src/router/index.ts`).
- State management / ESLint / testing are NOT yet implemented — confirm with user before introducing.
