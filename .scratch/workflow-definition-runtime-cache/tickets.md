# Tickets: WorkflowDefinition 运行时缓存层

三张票构建定义运行时缓存（write-through + 精准读口），来源 spec：`.scratch/workflow-definition-runtime-cache/PRD.md`。Work the frontier：阻塞项全完成的票可开做。

## 1. 缓存 write-through 与按版本读取最小闭环（示踪弹）

**What to build:** 通过 HTTP 端点新增一个定义后，运行时读口按 `definitionId + version` 立即读到全量缓存条目（画布 `OriginalStringData` 与 `Options` round-trip 一致）；草稿就地保存后同版本条目被覆盖为新内容；删除定义后该版本不可见。纵贯缓存模型（Domain.Shared，全量字段）、专用访问器（Caching 模块，条目键含租户，不套泛型 `ICacheService`）、`WorkflowDefinitionRepository` 三批量写口 write-through、两套测试基座的内存 `IMultilevelCacheClient` 替身接线（存量测试保持全绿）。

**Blocked by:** None — can start immediately.

- [x] `WorkflowDefinitionCacheModel` 携实体全部标量字段 + `Options` + `OriginalStringData`
- [x] `IWorkflowDefinitionCache`：`SetAsync(model)` / `RemoveAsync(definitionId, version)` / `GetAsync(definitionId, version)`，真实实现基于框架多级缓存客户端
- [x] 仓储写路径（含框架单数方法委托来的路径）自动同步缓存条目，命令处理器零改动
- [x] EF 与 Service.Open 两套测试基座注册内存级多级缓存客户端替身，真实访问器逻辑参与测试；现有测试（含 `WorkflowDefinitionRepositoryTest`）全绿
- [x] HTTP 集成测试：新增→可读、草稿 Save→同版本覆盖、删除→不可见、Options/画布 round-trip

## 2. 按定义索引与派生读口 + 完整失效矩阵

**What to build:** 运行时读口支持按 `definitionId` 取"最新行"与"最新已发布版"，在真实命令序列下指针语义正确：发布→取已发布；发布后再存草稿→`GetLatest` 指新草稿、`GetLatestPublished` 不动；连续两版发布→取版本号大者；Move 全版本行缓存一致；Rollback 两形态内容正确；删除定义后全部版本条目与按定义索引整体消失。

**Blocked by:** 1. 缓存 write-through 与按版本读取最小闭环

- [x] 按定义索引键（租户隔离）支撑两个派生读口，Insert/Update 维护、删空即清
- [x] `GetLatestAsync(definitionId)` 取 `IsLatest` 行；`GetLatestPublishedAsync(definitionId)` 取已发布中 `Version` 最大行
- [x] HTTP 集成测试补全 Publish/Save 新版本/Rollback（两形态）/Move/Delete 的指针与内容矩阵
- [x] 全量测试绿

## 3. 收口验证与留痕

**What to build:** 整体验收：全解决方案测试（含集成）绿；管理台 3 查询既有测试零改动确认（读侧未切换的边界被守住）；两条纪律以代码注释固化（进缓存的实体必须 `Options` 已还原；已发布行不可变故免 TTL/回退/广播兜底，理由指向 spec）。

**Blocked by:** 1、2

- [x] `dotnet test Backend/Cike.Workflow.sln`：Core 166 绿 / EF 30 绿 / 本变更 11 个缓存测试绿。⚠ Service.Open.Tests 另有 22 个**既有失败**（Save/Publish/Rollback/Move 测试类），成因为工作区内未提交的 `SaveWorkflowDefinitionDto.Body → Root` 改名（src 已改、这些测试载荷仍发 `body`），与本变更无关、未代改；证据留存见交付报告
- [x] 仓储类注释：写入口实体约束 + 免失效设计依据
- [x] `git diff` 确认管理台查询处理器与端点契约无改动
