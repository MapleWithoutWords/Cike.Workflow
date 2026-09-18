using Cike.Workflow.Core.Models;
using Cike.Workflow.Domain.Shared.CacheModels;

namespace Cike.Workflow.Caching;

/// <summary>
/// 工作流定义运行时缓存专用访问器：唯一读口按 <see cref="WorkflowDefinitionHandle"/> 寻址单条版本行——
/// DefinitionId 路径经 <see cref="Cike.Workflow.Common.Versions.VersionOptions"/> 解析目标版本
/// （SpecificVersion / Latest / Published / LatestOrPublished 等，为空时取版本号最大行）；
/// DefinitionVersionId 路径按行 Id 直接寻址。
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
    /// <summary>写入（或按版本覆盖）一条定义缓存，并维护行 Id → DefinitionId 影射。</summary>
    Task SetAsync(WorkflowDefinitionCacheModel model, CancellationToken cancellationToken = default);

    /// <summary>移除指定版本条目并清理其行 Id 影射；定义条目清空时连定义条目一并移除。</summary>
    Task RemoveAsync(string definitionId, int version, CancellationToken cancellationToken = default);

    /// <summary>按 handle 寻址读取单条版本行（实例钉版本执行 / 触发新实例 / 草稿调试的统一读口）。</summary>
    Task<WorkflowDefinitionCacheModel?> GetAsync(WorkflowDefinitionHandle handle, CancellationToken cancellationToken = default);
}
