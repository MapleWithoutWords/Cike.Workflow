using Cike.Contracts.Extensions;
using System.Linq.Dynamic.Core;

namespace Cike.Workflow.Domain.Shared.Stores;

public abstract class BaseStore<TEntity>(CikeWorkflowDbContenxt context) : IBaseStore<TEntity>, IScopedDependency where TEntity : class, IEntity<long>
{
    public IQueryable<TEntity> Queryable => context.Set<TEntity>();

    public virtual async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filter, string sorting = "CreatedAt desc", CancellationToken cancellationToken = default)
    {
        var query = Queryable.AsNoTracking().Where(filter).OrderBy(sorting);
        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TResult>> GetListAsync<TResult>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TResult>> selector, string sorting = "CreatedAt desc", CancellationToken cancellationToken = default)
    {
        var query = Queryable.AsNoTracking().Where(filter).OrderBy(sorting);
        return await query.Select(selector).ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FindAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Queryable.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<(long Total, List<TEntity> Items)> ToPaginationAsync(IQueryable<TEntity> query, IPagedAndSortedRequest pageAndSorted, CancellationToken cancellationToken = default)
    {
        return await query.ToPaginationAsync(pageAndSorted, cancellationToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await AddRangeAsync([entity], cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await context.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity == null)
            return;
        await this.DeleteAsync(entity, cancellationToken);
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await this.DeleteRangeAsync([entity], cancellationToken);
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        var entities = await context.Set<TEntity>().Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
        await this.DeleteRangeAsync(entities, cancellationToken);
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        context.RemoveRange(entities);
        await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await this.UpdateRangeAsync([entity], cancellationToken);
        return entity;
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        context.UpdateRange(entities);
        await context.SaveChangesAsync(cancellationToken);
    }
}

public abstract class BaseStore<TEntity, TCacheModel>(CikeWorkflowDbContenxt context, ICacheService<TCacheModel> cacheService) : BaseStore<TEntity>(context), IScopedDependency
    where TEntity : class, IEntity<long>
     where TCacheModel : EntityDto<long>, IMultiTenant
{
    public override async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await base.AddRangeAsync(entities, cancellationToken);
        await cacheService.SetListAsync(entities.Select(e => e.Adapt<TCacheModel>()), cancellationToken);
    }

    public override async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await base.DeleteRangeAsync(entities, cancellationToken);
        await cacheService.RemoveRangeAsync(entities.Select(e => e.Id).ToList(), cancellationToken);
    }

    public override async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await base.UpdateRangeAsync(entities, cancellationToken);
        await cacheService.SetListAsync(entities.Select(e => e.Adapt<TCacheModel>()), cancellationToken);
    }
}
