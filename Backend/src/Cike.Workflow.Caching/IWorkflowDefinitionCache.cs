using Cike.Workflow.Domain.Shared.CacheModels;

namespace Cike.Workflow.Caching;

/// <summary>
/// 工作流定义运行时缓存专用访问器：按 DefinitionId + Version 复合键直接寻址全量内容，
/// 辅以按定义的版本索引支撑"最新行 / 最新已发布版"派生读口。
/// <para>
/// 不套泛型 <see cref="ICacheService{TModel}"/>：后者的 GetListAsync 是全量拉取再内存过滤，
/// 会把全部版本行的画布 JSON 拖进每次运行时读取。消费方是 WorkflowRuntime；
/// 管理台读链路（目录列表 / 详情 / 版本列表）不经过本接口，仍走数据库。
/// </para>
/// <para>
/// 写口（Set / Remove）只由 <c>WorkflowDefinitionRepository</c> 的 write-through 调用，
/// 业务代码不得直接写缓存。
/// </para>
/// </summary>
public interface IWorkflowDefinitionCache
{
    /// <summary>写入（或按版本覆盖）一条定义缓存，并维护该定义的版本索引。</summary>
    Task SetAsync(WorkflowDefinitionCacheModel model, CancellationToken cancellationToken = default);

    /// <summary>移除指定版本条目，并从索引中摘除；索引清空时连索引一并移除。</summary>
    Task RemoveAsync(string definitionId, int version, CancellationToken cancellationToken = default);

    /// <summary>按定义编号 + 版本号直接寻址读取（实例钉版本执行的读口）。</summary>
    Task<WorkflowDefinitionCacheModel?> GetAsync(string definitionId, int version, CancellationToken cancellationToken = default);

    /// <summary>取该定义的 IsLatest 行（可能是草稿；调试链路 Application 层建实例后交调度使用）。</summary>
    Task<WorkflowDefinitionCacheModel?> GetLatestAsync(string definitionId, CancellationToken cancellationToken = default);

    /// <summary>取该定义已发布行中 Version 最大者（触发新实例的解析读口）。</summary>
    Task<WorkflowDefinitionCacheModel?> GetLatestPublishedAsync(string definitionId, CancellationToken cancellationToken = default);
}
