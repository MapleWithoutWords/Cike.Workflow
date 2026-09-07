using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Domain.Data;

public interface IBaseStore<TEntity> where TEntity : class, IEntity<long>
{
    IQueryable<TEntity> Queryable { get; }

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
