using Cike.Auth.MultiTenant;
using Cike.Contracts.EntityDtos;
using System.Linq.Expressions;

namespace Cike.Workflow.Realtime.Tests.Infrastructure;

/// <summary>
/// ICacheService 内存替身：以 Singleton 注册（模拟 Redis 的跨 Scope 可见性），
/// 测试宿主因此不依赖外部 Redis。缓存同步行为（写 set / 删 remove）对测试可观察。
/// </summary>
public class InMemoryCacheService<TModel> : ICacheService<TModel>
    where TModel : EntityDto<long>, IMultiTenant
{
    private readonly Dictionary<long, TModel> _store = new();

    public Task<TModel?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var model);
        return Task.FromResult(model);
    }

    public Task<IEnumerable<TModel>> GetListAsync(Expression<Func<TModel, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<TModel> result = _store.Values;
        if (filter != null)
        {
            result = result.Where(filter.Compile());
        }
        return Task.FromResult(result);
    }

    public Task RemoveAsync(long id, CancellationToken cancellationToken = default)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        foreach (var id in ids)
        {
            _store.Remove(id);
        }
        return Task.CompletedTask;
    }

    public Task SetAsync(TModel model, CancellationToken cancellationToken = default)
        => SetListAsync([model], cancellationToken);

    public Task SetListAsync(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
    {
        foreach (var model in models)
        {
            _store[model.Id] = model;
        }
        return Task.CompletedTask;
    }
}
