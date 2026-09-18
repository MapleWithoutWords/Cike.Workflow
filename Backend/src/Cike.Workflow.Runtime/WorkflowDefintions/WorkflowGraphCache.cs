using Cike.Core.DependencyInjection;
using Cike.Workflow.Core.WorkflowGraphs.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Cike.Workflow.Runtime.WorkflowDefintions;

/// <summary>
/// WorkflowGraph 进程内缓存：按定义版本行 Id 寻址，滑动过期 30 分钟——每次命中都刷新剩余时间，
/// 只有连续 30 分钟无人访问才会淘汰。已发布版本行不可变，图可安全常驻；草稿行的编辑不主动
/// 失效本缓存，最迟由滑动过期兜底。
/// 并发未命中不排序：物化是幂等纯计算，重复构建一次无害，不为它引入按键锁。
/// </summary>
internal class WorkflowGraphCache : ISingletonDependency
{
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(30);

    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public async Task<WorkflowGraph> GetOrCreateAsync(long definitionVersionId, Func<Task<WorkflowGraph>> factory)
    {
        if (_cache.TryGetValue(definitionVersionId, out WorkflowGraph? graph) && graph != null)
            return graph;

        var created = await factory();
        _cache.Set(definitionVersionId, created, new MemoryCacheEntryOptions { SlidingExpiration = SlidingExpiration });
        return created;
    }
}
