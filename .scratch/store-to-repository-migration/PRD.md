# Spec: Store 迁移为 Cike.Framework 自定义仓储

## Problem Statement

后端数据访问层自建了一套 `BaseStore` 体系（1 个基类 + 1 个带缓存的基类 + 8 个具体 Store），与 Cike.Framework 已提供的仓储能力（`IRepository` / `EfCoreRepository`）大面积重复。自建体系缺少框架仓储的既有能力：写方法的 `autoSave` 参数（可选延迟保存——跳过立即 `SaveChanges`，与框架环境事务正交组合，仓储本身不感知工作单元）、软删自动转换、`GetAsync`（不存在即抛 `UserFriendlyException`）、`AnyAsync` / `GetCountAsync` / `WithDetails`、CQRS 查询侧 `IReadOnlyRepository` 编译期只读约束。同时"Store"命名偏离框架的"仓储"惯例，开发者要维护两套数据访问心智模型，框架文档的能力也无法直接套用。此外，现状影子属性序列化只覆盖插入与单查路径，`Update` 路径完全缺失——经 `Update` 修改 `Options` / `WorkflowState` 等会被静默丢弃（当前仅因调用方 DTO 不含这些字段而未暴露）。

## Solution

将 7 个带附加行为的 Store 迁移为框架自定义仓储——缓存同步的 Workspace / Folder、影子属性序列化的 WorkflowDefinition / WorkflowInstance / Bookmark / ActivityInstanceExecutionRecord：继承 `EfCoreRepository<CikeWorkflowDbContenxt, TEntity, long>`、声明 `IXxxRepository : IRepository<TEntity, long>` 业务接口、标注 `IScopedDependency` 走约定注册并天然覆盖同实体的默认仓储。`BookmarkQueueItem` 无任何附加行为，删除其接口与实现，直接使用框架默认仓储（`AddCikeDbContext` 反射 DbSet 自动注册）。删除自建 `BaseStore` / `IBaseStore`。**外部行为保持不变，唯一例外：补齐 `Update` 路径的影子属性序列化**（修复现状静默丢失缺陷，见 Implementation Decisions）；其余——缓存同步、影子属性插入/读取路径、排序默认值、删除幂等语义、API 契约——均与迁移前一致。

## User Stories

1. 作为后端开发者，我希望 Workspace / Folder / WorkflowDefinition 的数据访问通过框架仓储接口进行，以便获得与框架统一的仓储心智模型和文档支持。
2. 作为后端开发者，我希望自定义仓储通过约定注册覆盖同实体的默认仓储，以便注入 `IWorkspaceRepository` 与注入 `IRepository<Workspace, long>` 解析到同一个实现。
3. 作为后端开发者，我希望查询处理器可以注入 `IReadOnlyRepository`（编译期只读），以便 CQRS 查询侧获得防误写的约束（新增能力，不强制现有 Handler 改用）。
4. 作为后端开发者，我希望保留带 `sorting` 参数的 `GetListAsync` 重载且默认排序仍为 `CreatedAt desc`，以便 WorkflowDefinition 列表查询调用方行为不变。
5. 作为后端开发者，我希望分页查询语义（总数 + 当前页数据）不变，以便工作区 / 文件夹 / 流程定义列表的分页结果与迁移前一致。
6. 作为后端开发者，我希望按 id 删除不存在实体时静默返回（幂等）的行为不变，以便调用方无需预检存在性。
7. 作为后端开发者，我希望 Workspace / Folder 写操作后的缓存同步（写后 set、删后 remove）行为不变，以便缓存一致性语义与迁移前完全一致。
8. 作为后端开发者，我希望 WorkflowDefinition / WorkflowInstance / Bookmark / ActivityInstanceExecutionRecord 的影子属性（JSON 列）在插入与读取路径上的序列化行为与迁移前一致，以便数据库中的 JSON 载荷格式与还原行为不变（Update 路径的修复见第 16 条）。
9. 作为后端开发者，我希望序列化失败时的容错行为（记日志、回退默认值）不变，以便损坏的历史数据不会让查询抛异常。
10. 作为后端开发者，我希望引擎侧 4 个实体（WorkflowInstance / Bookmark / BookmarkQueueItem / ActivityInstanceExecutionRecord）同样完成迁移——3 个带序列化的用自定义仓储、BookmarkQueueItem 直接用框架默认仓储，以便工作流引擎后续接入时直接使用框架仓储而不必二次迁移。
11. 作为后端开发者，我希望删除 `BaseStore` 后代码库中不再残留任何 `IBaseStore` 引用，以便不会出现两套数据访问抽象并存。
12. 作为 API 消费者（前端），我希望所有端点的路由、参数校验、响应结构、long→string 序列化完全不变，以便前端无需任何调整。
13. 作为 CI / 运维，我希望仓储层测试使用 SQLite in-memory、不依赖外部 MySQL / Redis，以便测试在任何环境（含 CI）都能运行。
14. 作为后端开发者，我希望测试标准文档（docs/testing.md）的数据库策略章节从"待定"更新为已定方案，以便后续 EF Core 相关测试有章可循。
15. 作为代码评审者，我希望调用方（Handler）的改动仅为机械替换（接口名、`Queryable` → `GetQueryable()`、`ToPaginationAsync` → `GetPagedListAsync`），以便评审可以快速确认无行为变化。
16. 作为后端开发者，我希望 `Update` 路径同样执行影子属性序列化，以便经 `Update` 修改的 `Options` / `WorkflowState` / `Payload` 等不再被静默丢弃。
17. 作为后端开发者，我希望没有附加行为的实体直接使用框架默认仓储，以便不为纯 CRUD 维护多余的接口与实现类。

## Implementation Decisions

- **迁移范围**：7 个带附加行为的 Store 迁移为自定义仓储（Workspace、Folder——缓存同步；WorkflowDefinition、WorkflowInstance、Bookmark、ActivityInstanceExecutionRecord——影子属性序列化）及其接口；`BookmarkQueueItemStore` / `IBookmarkQueueItemStore` 无任何 override，直接删除，消费方注入框架默认仓储。判断标准：**有影子属性序列化或缓存同步等附加行为才建自定义仓储，纯 CRUD 实体一律默认仓储**。删除 `BaseStore`（含带缓存泛型版本）与 `IBaseStore`。
- **自定义仓储形态**：实现类继承框架 `EfCoreRepository<CikeWorkflowDbContenxt, TEntity, long>`，实现 `IXxxRepository`，标注 `IScopedDependency` 走约定注册；约定注册先于 `AddCikeDbContext` 的 TryAdd 默认注册执行，因此天然覆盖同实体默认仓储，无需额外配置。
- **接口位置与命名**：`IXxxStore` → `IXxxRepository`，继承 `IRepository<TEntity, long>`；接口仍定义在 Domain 层（框架 `IRepository` 位于 `Cike.Data.Domain` 包，无 EF Core 依赖，Domain 层可直接引用）；实现类仍在 EntityFrameworkCore 层（框架边界：EF Core 只属于该层）。
- **缓存同步（Workspace / Folder）**：在自定义仓储中 override 框架写方法，保留"写库成功后同步缓存"的现状语义。框架的单数写方法内部**委托批量方法**（`InsertAsync → InsertManyAsync(new[]{entity})`，Update/Delete 同理），因此**只需覆写批量方法**即可覆盖全部写路径；若两组都覆写，单实体写会触发两次缓存同步。
- **影子属性序列化**：写路径覆盖**插入与更新**——框架单数写方法内部委托批量方法，覆写批量方法（InsertManyAsync / UpdateManyAsync）即可同时覆盖单实体与批量两条路径。其中 Update 路径为**本次修复项**：现状完全缺失，经 `Update` 修改 `Options` / `WorkflowState` / `Payload` 等会被静默丢弃，迁移时统一补齐**全部 4 个序列化仓储**（WorkflowDefinition / WorkflowInstance / Bookmark / ActivityInstanceExecutionRecord）的 Update 序列化。读路径按各 Store 现状对齐——WorkflowDefinition / WorkflowInstance / ActivityInstanceExecutionRecord 仅 `FindAsync` 反序列化，BookmarkStore 额外覆盖 `GetListAsync` 反序列化，其余列表方法不反序列化（读侧缺口保持现状，见 Out of Scope）。序列化失败容错（记日志回退默认值）保留。
- **排序兼容重载**：框架 `GetListAsync(predicate)` 无排序参数；保留 `GetListAsync(filter, sorting = "CreatedAt desc")` 重载，排序经 `System.Linq.Dynamic.Core` 生效、默认值与现状一致。仅需两处：`WorkflowDefinitionRepository`（现有唯一调用方）与 `BookmarkRepository`（其列表反序列化 override 依赖该签名）。现状的投影重载（带 selector）**无任何调用方，不保留**。
- **分页**：`ToPaginationAsync(query, request)` 由框架 `GetPagedListAsync(request, predicate)` 提供；调用方从"Queryable + ToPaginationAsync"机械替换为"GetQueryable().Where(...).AsNoTracking() + GetPagedListAsync"，分页语义不变。
- **跟踪行为**：遵循框架默认（跟踪查询）。现状 `BaseStore.FindAsync` / `GetListAsync` 强制 `AsNoTracking`、`Queryable` 属性为跟踪查询；迁移后只读场景由调用方继续显式 `AsNoTracking()`（机械替换），`FindAsync` 后接 `Update` / `Delete` 的场景在两种跟踪模式下最终落库等价。
- **autoSave 语义**：调用方不传 `autoSave`（默认 true，立即保存），与现状"每个写方法立即 SaveChanges"一致；不借机改造为 UoW 事务模式。
- **审计字段 / 主键 / 多租户**：由框架 `CikeDbContext` 钩子自动填充，与现状一致。
- **新增测试项目** `Cike.Workflow.EntityFrameworkCore.Tests`（NUnit 3 + NSubstitute，目录镜像被测命名空间，规范见 docs/testing.md）。
- **测试数据库策略（本次敲定，更新 docs/testing.md 第 7 节）**：SQLite in-memory 共享单连接，测试基座仿框架 `CikeEfCoreTestHost`——模块加载（`AddApplicationAsync<TModule>`）与生产一致，通过 `Configure<CikeDbContextOptions>` 注入 `UseSqlite(共享连接)` 覆盖 MySQL 方言，`EnsureCreatedAsync` 建表；不引入框架 MySql Provider。
- **json 列类型兼容**：实体配置中影子属性使用 `HasColumnType("json")`（MySQL 方言）。若 SQLite 建表 / 映射不接受该列类型，在测试基座侧处理（如按 provider 替换列类型），**不改动生产 MySQL 映射**。
- **缓存替身**：测试中 `ICacheService<TModel>` 用内存实现替身（不依赖 Redis），可观察 set / remove 调用。

## Testing Decisions

- **好测试的标准**：只断言外部可观察行为——CRUD 返回值与持久化结果、序列化 round-trip、缓存同步可见性、DI 解析结果；不测实现细节（不 mock 被测仓储、不断言内部调用序列），使后续重构不需要改测试。
- **被测模块**：`Cike.Workflow.EntityFrameworkCore.Tests` 覆盖全部 7 个自定义仓储，并断言 BookmarkQueueItem 解析到框架默认仓储；API 层现有测试（Service.Open.Tests）不改动，作为端点契约不变的既有保护。
- **测试类型**：
  - **DI 覆盖解析**：`IXxxRepository` 与 `IRepository<TEntity, long>`、`IReadOnlyRepository<TEntity, long>` 解析到同一自定义仓储类型（先例：框架 `CustomRepositoryTests`）；`IRepository<BookmarkQueueItem, long>` 解析到框架默认 `EfCoreRepository`（确认未被误覆盖）。
  - **CRUD 行为**：插入返回已填充主键的实体、`FindAsync` 幂等查找、按 id 删除不存在的记录静默返回、分页返回正确的 Total / Items。
  - **影子属性序列化 round-trip**：插入后影子属性列写入预期 JSON；**更新后影子属性列写入新值 JSON（本次修复的行为，4 个序列化仓储均需覆盖）**；`FindAsync`（BookmarkStore 含 `GetListAsync`）还原对象图与原始值等价；损坏 JSON 时记日志回退、不抛异常。
  - **缓存同步**：Workspace / Folder 写后缓存被 set、删后缓存被 remove（内存替身可观察）。
  - **排序兼容重载**：`GetListAsync(filter, sorting)` 按指定排序生效，默认 `CreatedAt desc`。
- **先例**：框架 `Cike.Data.EFCore.Tests`（`WriteTests` / `ReadOnlyRepositoryTests` / `CustomRepositoryTests` / 测试基座 `CikeEfCoreTestHost`）；本项目 docs/testing.md（NUnit、大驼峰三段式命名 `Method_Scenario_Expected`、`Assert.That` 约束模型、AAA 排列、`[Category("Integration")]` 标注数据库类测试）。
- 数据库类测试标注 `[Category("Integration")]`，与全量 / 单元测试过滤命令兼容。

## Out of Scope

- 不修复现状"列表查询不反序列化影子属性"（除 BookmarkStore 已覆盖的 `GetListAsync`）——保持现状，留待引擎接入时统一设计。
- 不引入 UoW（`autoSave: false` + 事务提交）改造调用方。
- 不改缓存架构（Redis 配置、缓存模型、失效策略）。
- 不改 API 契约、DTO、路由与校验规则。
- 不涉及前端。
- 不迁移 / 不验证 MySQL 方言特有行为（json 列类型的真实 DDL 以生产为准）。

## Further Notes

- 引擎侧 4 个实体（WorkflowInstance / Bookmark / BookmarkQueueItem / ActivityInstanceExecutionRecord）当前**无任何消费方**（属预建）：3 个序列化仓储由 round-trip 测试保护；BookmarkQueueItem 走默认仓储，由解析断言保护。
- 4 个序列化 Store 的现状差异需逐一对齐：WorkflowDefinition（`SerializedOptions`）、WorkflowInstance（`SerializedWorkflowState`）、ActivityInstanceExecutionRecord（6 个影子属性、空集合写 null）、BookmarkStore（`SerializedPayload` / `SerializedMetadata`，null 安全 + 列表反序列化）。Update 路径序列化为本次统一补齐项，4 个仓储一致覆盖、不保留差异。
- 现状 `BaseStore.DeleteRangeAsync(ids)` 是"加载实体再删除"，框架 `DeleteAsync(id)` 为 `FindAsync` 后删除且幂等——语义等价。
- 基座需注意：`CikeWorkflowEntityFrameworkCoreModule` 依赖 `CikeWorkflowCachingModule`（Redis），测试模块需以缓存替身替换，避免测试宿主连 Redis。
