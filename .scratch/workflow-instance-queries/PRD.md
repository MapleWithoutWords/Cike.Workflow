# PRD: 工作流实例分页查询与详情查询

Status: ready-for-agent

## Problem Statement

管理台目前只能"发起/取消/恢复"工作流实例，但看不到任何实例数据：没有实例列表（无法按状态、定义、关联 ID、时间段等条件筛选分页浏览），也没有实例详情（无法查看实例当前状态快照和各活动的执行记录）。运维与开发人员排查"某个流程跑没跑、跑到哪、为什么出错"时没有任何入口。

## Solution

提供两个只读查询端点：

1. **实例分页列表**：以既有的 `WorkflowInstanceFilter` 全量过滤能力（ID 集合、名称、定义 ID/版本、关联 ID、状态（细粒度与主状态）、是否执行中、是否有事故、是否系统实例、时间戳区间过滤、综合搜索词）+ 标准分页排序参数，返回实例条目（含定义名称、状态、是否执行中、事故数、完成时间、审计字段）。
2. **实例详情**：按实例 ID 返回列表条目的全部字段，外加实例完整状态快照（WorkflowState）与该实例全部活动执行记录（状态、输入输出、异常、调用栈深度等）。

## User Stories

1. 作为管理台用户，我想分页浏览全部工作流实例，以便总览系统的流程运行情况。
2. 作为管理台用户，我想按实例状态（如运行中/完成/故障）筛选实例，以便快速定位某一类流程。
3. 作为管理台用户，我想按主状态（WorkflowMainStatus，如进行中/已结束/异常）粗粒度筛选，以便不用记住细粒度状态枚举也能筛选。
4. 作为管理台用户，我想按工作流定义 ID 或定义版本筛选实例，以便查看某个流程定义的所有运行记录。
5. 作为管理台用户，我想按关联 ID（CorrelationId）筛选实例，以便把一次业务请求触发的整条流程链找出来。
6. 作为管理台用户，我想用综合搜索词（实例 ID、实例名、定义 ID、版本号、关联 ID 任意命中）搜索实例，以便只记得片段信息时也能找到目标。
7. 作为管理台用户，我想按创建/更新/完成时间做区间与单日过滤，以便查看某时间段内运行过的实例。
8. 作为管理台用户，我想筛选"有事故/无事故"的实例，以便优先处理出错的流程。
9. 作为管理台用户，我想筛选"执行中/未执行"的实例，以便区分活动中的与挂起的流程。
10. 作为管理台用户，我想区分系统实例与业务实例，以便排除系统内部流程的干扰。
11. 作为管理台用户，我想按父实例 ID 集合筛选子实例，以便从父流程下钻查看其子流程。
12. 作为管理台用户，我想按任意列排序并自定义每页条数，以便按自己的习惯浏览列表。
13. 作为管理台用户，我想在列表里直接看到定义名称（而不只是定义 ID），以便一眼认出实例属于哪个流程。
14. 作为管理台用户，我想打开一个实例查看它的完整状态快照（变量、书签、执行位置等），以便了解流程当前的精确处境。
15. 作为管理台用户，我想查看实例下每个活动的执行记录（状态、名称、输出、异常、完成时间、调用栈深度），以便定位是哪个活动出了问题。
16. 作为管理台用户，我想在活动记录里看到递归的异常信息（含内层异常），以便定位根因而非只看到表层报错。
17. 作为管理台用户，我想让详情在实例不存在时收到明确的业务错误提示，以便区分"ID 输错"与"接口故障"。
18. 作为管理台用户（前端开发者），我希望所有 long 型 ID 以字符串返回，以便避免 JS 精度丢失。
19. 作为管理台用户（前端开发者），我希望过滤条件走 POST body，以便复杂结构（状态数组、时间戳过滤列表）能自然表达。
20. 作为运维人员，我想让查询接口走与其他管理端点一致的认证要求，以便不产生未授权的数据暴露。

## Implementation Decisions

- **复用 Domain 层已有的 `WorkflowInstanceFilter`**：其 `Apply()` 已实现全部过滤逻辑（含时间戳过滤与综合搜索词），仓储接口已有以该 filter 查询的形态。端点 POST body 直接绑定该 filter 类型；不为 Contracts 层复制一份 30+ 字段的过滤 DTO（Contracts 不引用 Domain，复制会造成双源漂移）。
- **列表端点为 POST**：框架约定"复杂类型 → JSON body（GET 也走 body，因此 GET 端点不要声明复杂参数）"，filter 含集合与嵌套时间戳过滤，必须走 body。分页参数（Page/PageSize/Sorting）经 `[AsParameters]` 从查询串绑定，沿用既有列表端点形态。默认排序 `CreatedAt desc`。
- **分页机制**：filter.Apply(IQueryable) 后用框架的 `ToPaginationAsync`（先 LongCount 再排序 Skip/Take；Sorting 非空才 OrderBy）。查询在 `BeginAsNoTracking()` 只读范围内执行。
- **DefinitionName 关联填充**：实例表不冗余存定义名称；以 `DefinitionVersionId → WorkflowDefinition.Id` 批量查询版本行取 Name，一次额外查询填充两处 DTO（列表与详情）。
- **详情读路径**：经仓储 `FindAsync`（其读路径钩子会把影子属性里的序列化 JSON 反序列化回 `WorkflowState` 对象图）；活动执行记录按 `WorkflowInstanceId` 查询、`CreatedAt`+`Id` 升序；两组数据经 Mapster 映射到既有 DTO（`WorkflowInstanceItemDto` / `WorkflowInstanceDetailDto` / `ActivityInstanceExecutionRecordDto` 均已存在，无需新 DTO）。
- **查询记录（Query record）放 Application 层**、与既有 Workspaces/WorkflowDefinitions 垂直切片同构：handler 类挂 `[LocalEventHandler]`，端点经 `ILocalEventBus.PublishAsync` 派发。实例不存在时抛 `UserFriendlyException`（全局转 400 + 消息）。
- **新端点类**继承 `MinimalApiServiceBase`，方法命名按约定自动生成路由：`POST api/v1/WorkflowInstances/PagedList` 与 `GET api/v1/WorkflowInstances/{id}`；认证走全局默认（端点级 RequireAuthorization）。
- **无 schema 变更**：不新增实体、不新增迁移；纯粹在既有实体与仓储上叠加查询。

## Testing Decisions

- **好测试只测外部行为**：发真实 HTTP 请求，断言状态码与响应 JSON（过滤结果集、分页 Total/Items、DefinitionName 填充、详情的 WorkflowState 与活动记录、long→string 序列化、不存在 ID 的 400）。不断言 handler 内部实现。
- **测试缝（唯一）**：HTTP 集成缝——`Cike.Workflow.Service.Open.Tests` 的 `BaseIntegrationTest`（WebApplicationFactory + TestAuthHandler + 共享单连接 SQLite in-memory + 内存缓存替身）。不新增更低层的缝。
- **数据构造**：测试内经仓储插入实体（实例必须带非空 WorkflowState——写路径钩子序列化它；活动记录用仓储批量插入）。与既有测试一致：类独享宿主、用唯一数据标记隔离、标注 `[Category("Integration")]`。
- **先例**：`WorkflowDefinitionSaveTest` / `WorkflowDefinitionPublishTest` 等——同基座、同 HttpClient 断言风格；新测试完全跟随 `.claude/rules/testing.md` 的命名三段式与 `Assert.That` 约束模型。

## Out of Scope

- 实例控制操作（发起/取消/恢复）——已存在，本 spec 不动。
- 实例删除、批量操作、重试/终止等运维写操作。
- 聚合统计端点（按状态计数、按定义分布等）。
- 专用的"子实例列表"端点——已由 filter 的 ParentWorkflowInstanceIds 覆盖。
- 按 WorkspaceId 过滤——实体有该列，但实例写路径（WorkflowStateMapper）并不回填它，数据来源不可靠；待写路径补齐后另行扩展。
- 前端页面与任何 UI 改动。

## Further Notes

- `ToPaginationAsync` 的框架已知技术债：`Page <= 0` 行为未定义、`Sorting` 为空时无排序（分页结果不确定）——端点侧以默认值（Page/PageSize 合理默认 + Sorting 默认 `CreatedAt desc`）规避，不依赖框架修复。
- 综合搜索词的匹配语义由既有 `WorkflowInstanceFilter` 决定：Name/DefinitionId/CorrelationId 为 Contains，实例 ID/版本 ID 为精确等值；不在此 spec 内改动。
- 时间戳过滤支持"零时刻=整天"语义（如 `Is` + `2026-09-18T00:00:00` 过滤全天），列名白名单为 CreatedAt/UpdatedAt/FinishedAt。
- 实施完成后更新项目记忆：实例域"只有 DTO 无实现"的现状自此改变。
