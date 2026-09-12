namespace Cike.EntityFrameworkCore.Repositories;

/// <summary>
/// 带缓存同步的仓储基类：写库成功后同步缓存（写 set、删 remove），语义与迁移前的缓存版 BaseStore 一致。
/// 框架的单数写方法内部委托批量方法（InsertAsync → InsertManyAsync 等），只需覆写批量方法即可覆盖全部写路径。
/// </summary>
public abstract class CachedEfCoreRepository<TDbContext, TEntity, TCacheModel>(TDbContext dbContext, ICacheService<TCacheModel> cacheService)
    : EfCoreRepository<TDbContext, TEntity, long>(dbContext)
    where TDbContext : CikeDbContext<TDbContext>
    where TEntity : class, IEntity<long>
    where TCacheModel : EntityDto<long>, IMultiTenant
{
    public override async Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await base.InsertManyAsync(entities, autoSave, cancellationToken);
        await SetCacheAsync(entities, cancellationToken);
    }

    public override async Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await base.UpdateManyAsync(entities, autoSave, cancellationToken);
        await SetCacheAsync(entities, cancellationToken);
    }

    public override async Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await base.DeleteManyAsync(entities, autoSave, cancellationToken);
        await cacheService.RemoveRangeAsync(entities.Select(e => e.Id), cancellationToken);
    }

    private async Task SetCacheAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        await cacheService.SetListAsync(entities.Select(e => e.Adapt<TCacheModel>()), cancellationToken);
    }
}
