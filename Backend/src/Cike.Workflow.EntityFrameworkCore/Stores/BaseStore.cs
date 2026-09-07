namespace Cike.Workflow.Domain.Shared.Stores;

public abstract class BaseStore<TEntity>(CikeWorkflowDbContenxt context) : IBaseStore<TEntity> where TEntity : class, IEntity<long>, IScopedDependency
{
    public IQueryable<TEntity> Queryable => context.Set<TEntity>();

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await context.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await context.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity == null)
            return;
        await this.DeleteAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await this.DeleteRangeAsync([entity], cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        var entities = await context.Set<TEntity>().Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
        await this.DeleteRangeAsync(entities, cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        context.RemoveRange(entities);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await this.UpdateRangeAsync([entity], cancellationToken);
        return entity;
    }

    public async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        context.UpdateRange(entities);
        await context.SaveChangesAsync(cancellationToken);
    }
}
