# Tickets: Store 迁移为 Cike.Framework 自定义仓储

将 6 个带附加行为的 Store 迁移为框架自定义仓储、纯 CRUD 的 BookmarkQueueItem 交框架默认仓储、删除自建 BaseStore 体系，最后统一落地仓储测试。来源 spec：同目录 `PRD.md`。

Work the **frontier**: any ticket whose blockers are all done. 票①票②相互独立可并行；票③需等①②完成；票④需等③完成。

---

## ① 缓存类仓储迁移：Workspace + Folder

**What to build:** Workspace 与 Folder 的数据访问改走框架自定义仓储：接口改名为仓储约定并继承框架完整仓储契约，实现类继承框架 EF Core 仓储基类、以约定注册覆盖同实体默认仓储。缓存同步语义与迁移前一致——写库成功后 set 缓存、删库后 remove 缓存（框架单数写方法内部委托批量方法，覆写批量方法即可覆盖全部写路径）。4 个调用方（命令 + 查询 Handler）做机械替换：查询入口 `Queryable` → `GetQueryable()`、分页 `ToPaginationAsync` → `GetPagedListAsync`，业务逻辑不动。完成后删除两个旧 Store 及其接口。

**Blocked by:** 无 —— 可立即开始。

- [x] `IWorkspaceRepository` / `IFolderRepository` 定义在 Domain 层，继承框架 `IRepository<TEntity, long>`
- [x] 实现类继承框架 EF Core 仓储基类、标注 Scoped 约定注册，天然覆盖同实体默认仓储
- [x] 缓存同步：插入 / 更新后缓存被 set，删除后缓存被 remove（单实体 + 批量路径全覆盖）
- [x] Handler 机械替换完成，业务逻辑无变化；列表 / 详情 / 增删改端点行为与迁移前一致
- [x] 旧 Store 与接口删除，解决方案编译零残留引用
- [x] 现有 API 测试全部通过

## ② 序列化类仓储迁移：WorkflowDefinition / WorkflowInstance / Bookmark / ActivityInstanceExecutionRecord

**What to build:** 4 个带影子属性序列化的 Store 迁移为框架自定义仓储。写路径覆盖插入与更新——其中 **Update 路径序列化为本次修复项**（现状缺失，经 Update 修改 `Options` / `WorkflowState` / `Payload` 等会被静默丢弃；框架单数写方法委托批量方法，覆写批量方法即覆盖两条路径）；读路径按现状对齐：单查反序列化保留，Bookmark 额外保留列表查询反序列化，其余列表方法不反序列化（读侧缺口保持，见 PRD Out of Scope）。序列化失败容错（记日志回退默认值）保留。WorkflowDefinition 与 Bookmark 保留带排序参数的列表重载（默认 `CreatedAt desc`）；现状投影重载（带 selector）无调用方，不保留。WorkflowDefinition 的两个 Handler 机械替换，业务逻辑不动。完成后删除 4 个旧 Store 及其接口。

**Blocked by:** 无 —— 可与①并行。

- [x] 4 个仓储接口定义在 Domain 层，继承框架 `IRepository<TEntity, long>`；实现类约定注册覆盖默认仓储
- [x] 插入路径序列化：影子属性列写入预期 JSON（含 Bookmark / 执行记录的空值写 null 语义）
- [x] **Update 路径序列化补齐：更新后影子属性列写入新值 JSON，4 个仓储一致覆盖**
- [x] 读路径与现状对齐：单查反序列化；Bookmark 列表查询反序列化；其余列表不反序列化
- [x] 损坏 JSON 单查时记日志回退默认值、不抛异常
- [x] WorkflowDefinition / Bookmark 保留排序重载且默认排序生效；投影重载未保留
- [x] Handler 机械替换完成，业务逻辑无变化；定义相关端点行为与迁移前一致
- [x] 4 个旧 Store 与接口删除，解决方案编译零残留引用
- [x] 现有 API 测试全部通过

## ③ 收尾：BookmarkQueueItem 交默认仓储 + 删除 BaseStore 体系

**What to build:** 纯 CRUD 的 BookmarkQueueItem 不再维护自定义 Store——删除其接口与实现，消费方（当前无）今后直接注入框架默认仓储（`AddCikeDbContext` 反射 DbSet 自动注册）。同时删除自建 `BaseStore`（含带缓存泛型版本）与 `IBaseStore`，数据访问层只剩框架仓储一套抽象。

**Blocked by:** ① 缓存类仓储迁移、② 序列化类仓储迁移。

- [x] `BookmarkQueueItemStore` / `IBookmarkQueueItemStore` 删除，该实体经框架默认仓储可用（编译期可注入）
- [x] `BaseStore` / `IBaseStore` 删除，全解决方案 grep 零残留
- [x] 全解决方案编译通过、现有 API 测试全部通过

## ④ 统一测试：SQLite in-memory 测试基座 + 全部仓储测试

**What to build:** 新建 EntityFrameworkCore 层测试项目（NUnit，目录镜像被测命名空间，遵循 docs/testing.md 规范），搭建 SQLite in-memory 测试基座：与生产一致的模块加载方式、共享单连接、经框架 DbContextOptions 注入 SQLite 方言（`json` 列类型如有兼容问题在测试侧处理，不动生产 MySQL 映射）、缓存服务内存替身。在基座上一次写齐全部仓储测试，并把 docs/testing.md 第 7 节从"待定"更新为已落地方案。

**Blocked by:** ①②③（测试针对迁移完成后的最终仓储形态）。

- [x] 测试基座：模块加载与生产一致，SQLite in-memory 建表成功（含 json 影子属性列的方言兼容处理）
- [x] 缓存服务内存替身可注入，测试不依赖外部 MySQL / Redis
- [x] DI 覆盖解析：6 个自定义仓储与同实体 `IRepository` / `IReadOnlyRepository` 解析到同一实现类型
- [x] 默认仓储解析：BookmarkQueueItem 解析到框架默认 EF Core 仓储（未被误覆盖）
- [x] CRUD 行为：插入返回已填充主键、按 id 删除幂等、分页 Total / Items 正确
- [x] 序列化 round-trip：插入 / **更新（修复项断言）** / 单查还原（Bookmark 含列表查询）全绿
- [x] 缓存同步：Workspace / Folder 写后 set、删后 remove 可观察
- [x] 排序重载：指定排序生效、默认 `CreatedAt desc` 生效
- [x] docs/testing.md 第 7 节更新为 SQLite in-memory 方案，数据库类测试标注 Integration 分类
- [x] `dotnet test` 全量通过
