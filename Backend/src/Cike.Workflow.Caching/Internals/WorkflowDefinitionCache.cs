using Cike.Auth.MultiTenant;

namespace Cike.Workflow.Caching.Internals;

/// <summary>
/// 定义运行时缓存实现：条目键 = {租户}:{definitionId}:{version}（同版本覆盖写幂等）；
/// 索引键 = {租户}:{definitionId} → 在存版本号列表，支撑 GetLatest / GetLatestPublished 批取后内存选取。
/// 索引为读-改-写维护，与 BaseCacheService 的 id-list 同款取舍（同一定义并发写罕见）。
/// 键格式与 BaseCacheService 一致使用裸字符串键，多级缓存客户端的 TypeName 前缀策略负责类型隔离。
/// </summary>
internal class WorkflowDefinitionCache(IMultilevelCacheClient multilevelCacheClient, ICurrentTenant currentTenant)
    : IWorkflowDefinitionCache, IScopedDependency
{
    private string TenantPrefix => $"WorkflowDefinition:{currentTenant.Id.ToString()}";

    private string GetEntryKey(string definitionId, int version) => $"{TenantPrefix}:{definitionId}:{version}";

    private string GetIndexKey(string definitionId) => $"{TenantPrefix}:Index:{definitionId}";

    public async Task SetAsync(WorkflowDefinitionCacheModel model, CancellationToken cancellationToken = default)
    {
        await multilevelCacheClient.SetAsync(GetEntryKey(model.DefinitionId, model.Version), model);
        await AddToIndexAsync(model.DefinitionId, model.Version);
    }

    public async Task RemoveAsync(string definitionId, int version, CancellationToken cancellationToken = default)
    {
        await multilevelCacheClient.RemoveAsync<WorkflowDefinitionCacheModel>(GetEntryKey(definitionId, version));
        await RemoveFromIndexAsync(definitionId, version);
    }

    public async Task<WorkflowDefinitionCacheModel?> GetAsync(string definitionId, int version, CancellationToken cancellationToken = default)
    {
        return await multilevelCacheClient.GetAsync<WorkflowDefinitionCacheModel>(GetEntryKey(definitionId, version));
    }

    public async Task<WorkflowDefinitionCacheModel?> GetLatestAsync(string definitionId, CancellationToken cancellationToken = default)
    {
        return (await GetVersionsAsync(definitionId))
            .Where(e => e.IsLatest)
            .MaxBy(e => e.Version);
    }

    public async Task<WorkflowDefinitionCacheModel?> GetLatestPublishedAsync(string definitionId, CancellationToken cancellationToken = default)
    {
        return (await GetVersionsAsync(definitionId))
            .Where(e => e.IsPublished)
            .MaxBy(e => e.Version);
    }

    private async Task<List<WorkflowDefinitionCacheModel>> GetVersionsAsync(string definitionId)
    {
        var versions = await multilevelCacheClient.GetAsync<List<int>>(GetIndexKey(definitionId));
        if (versions == null || versions.Count == 0)
            return new List<WorkflowDefinitionCacheModel>();

        var entries = await multilevelCacheClient.GetListAsync<WorkflowDefinitionCacheModel>(
            versions.Select(v => GetEntryKey(definitionId, v)).ToArray());

        // 索引与条目短暂不一致（并发写窗口）时按缺失容忍，不把 null 抛给运行时
        return entries.Where(e => e != null).Select(e => e!).ToList();
    }

    private async Task AddToIndexAsync(string definitionId, int version)
    {
        var key = GetIndexKey(definitionId);
        var list = await multilevelCacheClient.GetAsync<List<int>>(key) ?? new List<int>();
        if (!list.Contains(version))
        {
            list.Add(version);
            await multilevelCacheClient.SetAsync(key, list);
        }
    }

    private async Task RemoveFromIndexAsync(string definitionId, int version)
    {
        var key = GetIndexKey(definitionId);
        var list = await multilevelCacheClient.GetAsync<List<int>>(key);
        if (list == null)
            return;

        list.Remove(version);
        if (list.Count == 0)
            await multilevelCacheClient.RemoveAsync<List<int>>(key);
        else
            await multilevelCacheClient.SetAsync(key, list);
    }
}
