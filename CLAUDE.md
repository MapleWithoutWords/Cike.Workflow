# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Layout

This is a full-stack workflow engine project split into two independent sub-projects:

- `Backend/` — .NET 8 solution (`Cike.Workflow.sln`)
- `Frontend/cike-workflow/` — Vue 3 + Vite frontend

---

## Backend

### Commands

```bash
# Build the whole solution
dotnet build Backend/Cike.Workflow.sln

# Run the API host
dotnet run --project Backend/src/Cike.Workflow.Service.Open

# Run all tests
dotnet test Backend/tests/Cike.Workflow.Test

# Run a single test class
dotnet test Backend/tests/Cike.Workflow.Test --filter "FullyQualifiedName~ExampleTest"

# Add an EF Core migration (run from repo root)
dotnet ef migrations add <MigrationName> --project Backend/src/Cike.Workflow.EntityFrameworkCore --startup-project Backend/src/Cike.Workflow.Service.Open

# Apply pending migrations
dotnet ef database update --project Backend/src/Cike.Workflow.EntityFrameworkCore --startup-project Backend/src/Cike.Workflow.Service.Open
```

### Architecture

The backend is built on the **Cike Framework** — an ABP-style modular framework. Every project exposes a `CikeModule` subclass that declares its dependencies via `[DependsOn([...])]`. Module initialization happens through `AddApplicationAsync` / `InitializeApplicationAsync` in `Program.cs`.

**Layer dependency chain (inner → outer):**

```
Domain.Shared
  └── Domain  (+ CikeCachingModule, CikeDomainModule)
        └── Application.Contracts  (+ CikeContractsModule)
              └── Application  (+ CikeCqrsModule, CikeEventBusLocalModule)
                    └── EntityFrameworkCore  (MySQL via CikeDataEFCoreMySqlModule)
                          └── Service.Open  (Minimal API host)
```

`Cike.Workflow.Core` is a **separate** layer that owns the workflow engine abstractions (`IActivity`, `Activity`, `ContainerActivity`). It has its own `CikeWorkflowCoreModule` but is not part of the above dependency chain — it is the domain-agnostic engine being built on top of standard .NET, not the ABP/Cike infrastructure.

**Namespace convention:** Each layer declares a shortened namespace that does **not** match the csproj name. Use the existing namespace in each `_Imports.cs` or module file as the canonical namespace for new files in that layer:

| Layer | Namespace |
|---|---|
| Domain.Shared | `Cike.Domain.Shared` |
| Domain | `Cike.Domain` |
| Application.Contracts | `Cike.Application.Contracts` |
| Application | `Cike.Application` |
| EntityFrameworkCore | `Cike.EntityFrameworkCore` |
| Service.Open | `Cike.Service.Open` |

**API endpoints** are registered as ASP.NET Core Minimal API handlers. New endpoints belong in `Backend/src/Cike.Workflow.Service.Open/Services/`. Endpoint paths follow the `/api/v1/{Resource}` convention.

**Persistence:** EF Core with MySQL. The `DbContext` is `CikeWorkflowDbContenxt` (inherits `CikeDbContext<T>`) — note the intentional typo in the class name (`Contenxt`); preserve it. The connection string key is `"Project"` in `appsettings.json`. Override via `appsettings.Development.json` for local dev.

**Shared package versions:** `Backend/Directory.Build.props` centralizes `TargetFramework`, `LangVersion`, and `CikeVersion` for all projects. Update versions there, not per-csproj.

**Runtime dependencies:** The application requires both MySQL and **Redis** at startup. Redis is configured under `RedisConfig` in `appsettings.json`. The checked-in `appsettings.json` (and `appsettings.Development.json`) contain placeholder values (`###`) — override with real values locally and do not commit credentials.

**Logging:** Serilog, rolling daily files written to `Logs/`.

**Validation:** FluentValidation — validators are auto-registered from the `Application` assembly.

**Swagger:** Registered via `AddCikeSwagger("Cike", ...)` in `CikeWorkflowServiceOpenModule`. Available at `/swagger` in development.

**Global usings:** Each layer has an `_Imports.cs` file that declares the project's global using directives. Add new namespace imports there rather than per-file.

### Integration Tests

Tests use **NUnit** (NUnit 3 + NUnit3TestAdapter). `Test1.cs` is an MSTest scaffold placeholder; write new tests with NUnit as shown in `ExampleTest.cs`.

Tests extend `BaseIntegrationTest`, which spins up a real `WebApplicationFactory<Program>`. Use `CreateClient()` for HTTP calls or inject services via `serviceProvider`.

Test method naming convention: `{Method}_{Condition}_{ExpectedResult}` with Chinese-language conditions (e.g., `Add_Name为空_返回400`).

Tests need their own `appsettings.Development.json` for database/Redis; the checked-in one is a template with placeholder values (`###`).

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
