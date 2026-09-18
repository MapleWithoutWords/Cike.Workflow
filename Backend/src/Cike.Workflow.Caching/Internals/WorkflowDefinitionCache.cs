using Cike.Auth.MultiTenant;
using Cike.Workflow.Common.Extensions;
using Cike.Workflow.Core.Models;

namespace Cike.Workflow.Caching.Internals;

/// <summary>
/// 定义运行时缓存实现：每个定义一个条目，键 = {租户}:Def:{definitionId} → 该定义全部版本行
/// （同版本覆盖写幂等），读出后在内存按 VersionOptions 解析——复用
/// <see cref="IVersionExtensions.WithVersion"/>，与 DB 查询同源语义（过滤 + Version 降序取首）。
/// 另设行 Id 影射键 = {租户}:VersionId:{definitionVersionId} → definitionId（单键写、无读-改-写），
/// 支撑 ByDefinitionVersionId 的 handle 读口。
/// 定义条目为读-改-写维护，与 BaseCacheService 的 id-list 同款取舍（同一定义并发写罕见）。
/// 键格式与 BaseCacheService 一致使用裸字符串键，多级缓存客户端的 TypeName 前缀策略负责类型隔离。
/// </summary>
internal class WorkflowDefinitionCache(IMultilevelCacheClient multilevelCacheClient, ICurrentTenant currentTenant)
    : IWorkflowDefinitionCache, IScopedDependency
{
    private string TenantPrefix => $"WorkflowDefinition:{currentTenant.Id.ToString()}";

    private string GetDefinitionKey(string definitionId) => $"{TenantPrefix}:Def:{definitionId}";

    private string GetVersionIdKey(long definitionVersionId) => $"{TenantPrefix}:VersionId:{definitionVersionId}";

    public async Task SetAsync(WorkflowDefinitionCacheModel model, CancellationToken cancellationToken = default)
    {
        var key = GetDefinitionKey(model.DefinitionId);
        var versions = await multilevelCacheClient.GetAsync<List<WorkflowDefinitionCacheModel>>(key) ?? [];

        // 覆盖写幂等：同版本行（罕见地含同版本不同 Id 的残留）就地替换
        versions.RemoveAll(e => e.Version == model.Version || e.Id == model.Id);
        versions.Add(model);
        await multilevelCacheClient.SetAsync(key, versions);

        await multilevelCacheClient.SetAsync(GetVersionIdKey(model.Id), model.DefinitionId);
    }

    public async Task RemoveAsync(string definitionId, int version, CancellationToken cancellationToken = default)
    {
        var key = GetDefinitionKey(definitionId);
        var versions = await multilevelCacheClient.GetAsync<List<WorkflowDefinitionCacheModel>>(key);
        if (versions == null)
            return;

        var removedIds = versions.Where(e => e.Version == version).Select(e => e.Id).ToList();
        versions.RemoveAll(e => e.Version == version);

        if (versions.Count == 0)
            await multilevelCacheClient.RemoveAsync<List<WorkflowDefinitionCacheModel>>(key);
        else
            await multilevelCacheClient.SetAsync(key, versions);

        foreach (var id in removedIds)
            await multilevelCacheClient.RemoveAsync<string>(GetVersionIdKey(id));
    }

    public async Task<WorkflowDefinitionCacheModel?> GetAsync(WorkflowDefinitionHandle handle, CancellationToken cancellationToken = default)
    {
        if (handle.DefinitionId == null && handle.DefinitionVersionId == null)
            throw new ArgumentException($"Invalid WorkflowDefinitionHandle: {handle}", nameof(handle));

        string? definitionId;
        if (handle.DefinitionId != null)
        {
            definitionId = handle.DefinitionId;
        }
        else
        {
            definitionId = await multilevelCacheClient.GetAsync<string>(GetVersionIdKey(handle.DefinitionVersionId!.Value));
            if (definitionId == null)
                return null;
        }

        var versions = await multilevelCacheClient.GetAsync<List<WorkflowDefinitionCacheModel>>(GetDefinitionKey(definitionId));
        if (versions == null || versions.Count == 0)
            return null;

        // DefinitionVersionId 路径：按行 Id 直接寻址（影射键已还原出 definitionId）
        if (handle.DefinitionId == null)
            return versions.FirstOrDefault(e => e.Id == handle.DefinitionVersionId);

        // DefinitionId 路径：VersionOptions 过滤 + Version 降序取首；为空时不过滤，取版本号最大行
        return versions.WithVersion(handle.VersionOptions ?? default).FirstOrDefault();
    }
}
