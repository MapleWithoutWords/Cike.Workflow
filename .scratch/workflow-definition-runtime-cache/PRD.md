# Spec: WorkflowDefinition 运行时缓存层

> Status: `ready-for-agent`

## Problem Statement

WorkflowInstance 运行时执行链路即将开工：引擎在创建实例、调度活动时会高频加载工作流定义（含画布内容 `OriginalStringData` 与变量 `Options`）。当前 `WorkflowDefinition` 完全裸查数据库——项目里已有的缓存模式（`ICacheService<T>` 的"id 列表 + 全量拉取后内存过滤"）只适合管理台小表列表查询，对运行时"按定义精准取一个版本"的热路径完全不适用：每次读取会把全表所有行连画布大 JSON 一起拉出来。同时，定义的写路径横跨 7 个命令（新增/改名/存草稿/发布/删除/移动/回滚），若缓存失效逻辑散落各命令处理器，遗漏一处即运行期读到旧定义。

## Solution

为 WorkflowDefinition 新增一个**面向运行时精准读取的缓存层**：全量内容缓存模型 + 专用访问器（按 `DefinitionId + Version` 直接寻址，辅以按定义的行索引支持"最新行 / 最新已发布版"两个派生读口）+ 仓储层 write-through（7 个写命令的失效路径全部经由 `WorkflowDefinitionRepository` 的写方法自动同步，命令处理器零改动）。

定位边界（已与需求方对齐）：**缓存只服务未来的 WorkflowRuntime**——管理台现有 3 个读查询（目录列表/详情/版本列表）保持走 DB 不切换；已发布版本行内容不可变（改动走新版本），运行时只读已发布缓存，因此不需要 TTL、回退与跨实例失效广播兜底；存量数据冷启动不做回退/预热，上线时手动清库重灌。

## User Stories

1. 作为 WorkflowRuntime（未来的引擎），我希望按 `DefinitionId + Version` 以 O(1) 加载指定版本的定义全文（含画布与 Options），以便实例钉住在创建时解析出的版本行上执行、不受后续发布影响。
2. 作为 WorkflowRuntime，我希望按 `DefinitionId` 获取"最新已发布版本"，以便外部按定义编号触发新实例时直接解析出可执行版本。
3. 作为 WorkflowRuntime 的调试入口（Application 层为草稿建实例后交调度），我希望按 `DefinitionId` 获取"最新行"（`IsLatest`，可能是草稿），以便调试链路也能从缓存取定义。
4. 作为后端开发者，我希望所有定义写操作（新增/改名/存草稿/发布/删除/移动/回滚）落库成功后缓存同步更新，以便运行时永远不需要感知失效逻辑、也不可能读到写库前内容。
5. 作为后端开发者，我希望"草稿就地更新"（未发布最新行被 Save 覆盖同版本）在缓存中同样覆盖对应版本条目，以便编辑器保存后的重新加载取到新内容。
6. 作为后端开发者，我希望"发布后 Save 产生 v+1 新草稿"时：新版本行写入缓存、`GetLatest` 解析到新草稿、`GetLatestPublished` 仍指向已发布的 v 版本，以便两个读口语义各自正确。
7. 作为后端开发者，我希望 `GetLatestPublished` 在该定义存在多个已发布版本时返回版本号最大的已发布行，以便"发布→存草稿→再发布"后指针自动跟到最新发布。
8. 作为后端开发者，我希望删除定义（连带其全部版本行）后，该定义在缓存中整体不可见（含行索引），以便运行期不再能加载已删除定义的新实例。
9. 作为后端开发者，我希望移动定义（全部版本行整体换目录）后缓存行与库内一致，以便行数据不因批量 Update 而与 DB 漂移。
10. 作为后端开发者，我希望回滚的两种形态（草稿覆盖、新建 v+1 草稿）都正确同步缓存，以便回滚后立即触发运行取到回滚后的内容。
11. 作为后端开发者，我希望缓存条目按租户隔离（与现有 Workspace/Folder 缓存的租户语义一致），以便 A 租户不可能命中 B 租户的定义缓存。
12. 作为后端开发者，我希望写进缓存的实体始终携带已还原的 `Options` 对象图（写路径实体经 `FindAsync` 取回，影子属性已反序列化），以便缓存 round-trip 不丢变量定义。
13. 作为 API 消费者（前端管理台），我希望目录列表/详情/版本列表的响应、语义与现在完全一致（继续走 DB），以便本次变更对管理台零感知。
14. 作为测试工程师，我希望缓存访问器的真实逻辑（键设计、索引维护、latest/latestPublished 选取）能跑在内存介质上的集成测试里、不依赖 Redis，以便 CI 任何环境可运行。
15. 作为维护者，我希望"已发布行不可变 + 运行时只读已发布"这一免失效兜底的设计理由记录在案，以便后人不误以为 TTL/回退是遗漏。
16. 作为运维，我希望"存量数据不回填、上线手动清库重灌"这一决策与其适用前提（项目尚处早期）明确记录，以便部署清单有据可依。
17. 作为未来的 WorkflowInstance 建模者，我希望本次就明确"实例按 `DefinitionId + Version` 钉版本"的对齐约束，以便实例表落库时与缓存寻址天然一致。

## Implementation Decisions

- **缓存模型**：新增 `WorkflowDefinitionCacheModel`（继承 `FullAuditedEntityDto<long>` + `IMultiTenant`，与 `FolderCacheModel` 同目录同风格），携带实体全部标量字段（`DefinitionId`/`Name`/`Description`/`Type`/`UsableAsActivity`/`MaterializerName`/`IsReadonly`/`IsSystem`/`Version`/`IsLatest`/`IsPublished`/`PublishedNote`/`PublishedBy`/`PublishedAt`/`WorkspaceId`/`FolderId`）**加 `Options` 值对象与 `OriginalStringData` 画布全文**——"定义的一切都进缓存"，runtime 拿到即可直接喂物化器。不缓存物化后的 `WorkflowActivity`（避免跨执行共享可变对象；反序列化成为热点再议）。
- **专用访问器，不套 `ICacheService<T>`**：在 Caching 模块新增 `IWorkflowDefinitionCache`（含实现，直接用框架 `IMultilevelCacheClient`）。原因：现有泛型模式的 `GetListAsync` 是全量拉取模型再内存过滤，会把全部版本行的画布 JSON 拖进每次运行时读取。既有 `BaseCacheService` 与 Folder/Workspace 模式完全不动、不改造。
  - 写口（仓储层专用）：`SetAsync(model)`、`RemoveAsync(definitionId, version)`。
  - 读口（runtime 消费）：`GetAsync(definitionId, version)`；`GetLatestAsync(definitionId)`；`GetLatestPublishedAsync(definitionId)`。
- **键设计**：条目键 = 租户 + `definitionId` + `version` 复合寻址（同版本就地更新即覆盖写，天然幂等）；索引键 = 租户 + `definitionId` → 该定义当前在存的全部版本号列表，支撑两个派生读口（批取版本条目后内存选 `IsLatest` 行 / `IsPublished` 中 `Version` 最大者）。具体键格式实现时与现有缓存键风格对齐，租户隔离语义与 `BaseCacheService` 现状一致。索引为读-改-写维护，与现有 id-list 同款取舍（同一定义并发写罕见，可接受）。
- **写侧收口在 `WorkflowDefinitionRepository` 内部，不建新基类**：注入 `IWorkflowDefinitionCache`，覆写 `InsertManyAsync` / `UpdateManyAsync` / `DeleteManyAsync` 三个批量写口（框架单数写方法内部委托批量方法，覆写批量即覆盖全部写路径——与 `SerializedEfCoreRepository` 既有注释同理），写库成功后同步条目与索引；Delete 后索引清空则连索引键一并移除。`WorkflowDefinitionRepository` 因此同时承担影子属性序列化钩子与缓存同步两种附加行为（放弃"组合基类"方案，需求方明确要求收在单类内）。
- **写路径实体约束**：进缓存的实体必须是 `Options` 已还原的行。现状全部写命令取行均经 `FindAsync`（含 `Move` 的逐行 `FindAsync`），满足约束；此约束以注释固化在仓储类上（同 `MoveWorkflowDefinitionCommandHandler` 里既有的影子属性警示注释口吻）。
- **免失效兜底的依据（记录进设计文档）**：已发布版本行不可变（Save 产生新版本而非改已发布行）；草稿/调试实例在 Application 层创建后才交调度，runtime 只读已发布条目。故不依赖多级缓存的跨实例 L1 失效同步；`Delete` 后其他实例 L1 短暂残留属已接受取舍（需求方拍板）。不引入 TTL、DB 回退、失效广播。
- **冷启动**：不做回退/预热/迁移工具；上线时手动清库重灌（需求方拍板，项目早期无存量顾虑）。
- **管理台读链路零改动**：3 个 QueryHandler、7 个 CommandHandler、端点契约均不动；无数据库 schema 变更，**不需要 EF 迁移**。
- **WorkflowInstance 对齐约束（本次不实现，仅记录）**：实例持久化 `DefinitionId + Version` 两列钉版本，与缓存寻址一致。

## Testing Decisions

- **好测试的标准**：只断言外部可观察行为——通过公共写路径后，从 `IWorkflowDefinitionCache` 读口的可见性、内容与指针语义（round-trip 后 `OriginalStringData` 与 `Options` 与写库内容一致）；不 mock 被测类型、不断言内部调用序列。
- **单一 seam（已与需求方确认）**：`Cike.Workflow.Service.Open.Tests` 既有 HTTP 集成基座（`BaseIntegrationTest`：真实模块组装 + SQLite in-memory + HttpClient 打端点）。写侧全部通过真实端点驱动 7 个写命令（不直接调仓储方法，符合 API 层测试纪律）；断言侧在测试宿主解析 `IWorkflowDefinitionCache` **真实实现**注入。
- **组合根替换点下沉一层**：测试注册内存版 `IMultilevelCacheClient` 替身（先例：现 `InMemoryCacheService` 替换 `ICacheService`；本次替换点下移到框架缓存客户端层），使键设计、索引 RMW、latest/latestPublished 选取的真实逻辑在测试中真实执行，而非替身自证。`ICacheService` 既有替身保持不动。
- **覆盖场景（每个写命令的失效矩阵 + 读口语义）**：
  - 新增/改名：写后按行可读、内容一致；
  - 存草稿（未发布）：同版本条目被覆盖为新画布；
  - 发布：`GetLatestPublished` 指向该行；发布后 Save 出 v+1 草稿：`GetLatest` 指草稿、`GetLatestPublished` 不变；连续两版本发布后 `GetLatestPublished` 取版本号大者；
  - 删除：全版本条目与索引不可见；
  - 移动：全部版本行缓存与库一致（`FolderId` 已更新）；
  - 回滚两形态（草稿覆盖 / 新草稿）内容正确；
  - 缓存内容 round-trip 含 `Options` 对象图还原；
  - 回归：管理台 3 查询既有测试不动、保持全绿。
- **标注与规范**：数据库类测试标 `[Category("Integration")]`；命名大驼峰三段式 `<被测方法>_<场景>_<预期>`；`Assert.That` 约束模型——均循 `.claude/rules/testing.md`。
- **不测**：缓存模型 DTO、模块声明类（纯声明代码）。
- **先例**：`Store-to-repository-migration` 落地的 `FolderRepositoryTest` / `WorkspaceRepositoryTest`（仓储写穿断言形态）、`Service.Open.Tests` 四个写操作 HTTP 集成测试（命令矩阵驱动方式）、两测试基座的 `InMemoryCacheService`（内存替身注入方式）。

## Out of Scope

- WorkflowRuntime / WorkflowInstance 执行链路与实例端点本身（本变更只交付其读口）。
- `WorkflowInstance` 表结构调整（`DefinitionId + Version` 钉版本仅作为对齐约束记录）。
- 物化产物（`WorkflowActivity` 对象）级缓存。
- 冷启动回填、预热钩子、存量迁移脚本。
- 管理台读查询切换到缓存（明确否决：管理台继续走 DB）。
- `ICacheService<T>` / `BaseCacheService` 通用模式的改造或泛化（含其 `GetListAsync` 在 id 列表缺失时的判空加固——Folder/Workspace 不受本变更影响）。
- 跨实例 L1 失效广播、TTL 策略。

## Further Notes

- 本 spec 由 2026-09-14 的对齐讨论产出：三项关键决策（缓存给 runtime 用 / 存量手动清库重灌 / 管理台不走缓存）与三项修正（读口按 definitionId+version、不建组合基类写侧收在仓储内、免担心 L1 同步）均已并入上文。
- 实施前建议补抓 Cike.Framework 缓存能力文档（`docs/ai` 路由，当前网络不可达）验证 `IMultilevelCacheClient` 的批取与序列化语义；实现以源码与文档为准，不凭记忆推测。
- 部署清单：本变更无 schema 变更、无迁移；上线动作 = 清库重灌（或接受旧行不进缓存直至手动处理）。
