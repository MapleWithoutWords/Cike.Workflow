using System.Linq.Expressions;

namespace Cike.Workflow.Domain.Data;

public interface IBaseStore<TEntity> where TEntity : class, IEntity<long>
{
    IQueryable<TEntity> Queryable { get; }

    Task<TEntity?> FindAsync(long id, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filter, string sorting = "CreatedAt desc", CancellationToken cancellationToken = default);

    Task<List<TResult>> GetListAsync<TResult>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TResult>> selector, string sorting = "CreatedAt desc", CancellationToken cancellationToken = default);

    Task<(long Total, List<TEntity> Items)> ToPaginationAsync(IQueryable<TEntity> query, IPagedAndSortedRequest pageAndSorted, CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
