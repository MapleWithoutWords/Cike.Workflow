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
