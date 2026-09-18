using Cike.Workflow.Runtime.Bookmarks.Models;
using System.Linq.Expressions;

namespace Cike.Workflow.Domain.Data;

public interface IBookmarkRepository : IRepository<BookmarkEntity, long>
{
    /// <summary>
    /// Returns the first bookmark matching the specified filter.
    /// </summary>
    ValueTask<BookmarkEntity?> FindAsync(BookmarkFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a set of bookmarks matching the specified filter.
    /// </summary>
    ValueTask<IEnumerable<BookmarkEntity>> FindManyAsync(BookmarkFilter filter, CancellationToken cancellationToken = default);

    ValueTask DeleteManyAsync(BookmarkFilter filter, CancellationToken cancellationToken = default);

    ValueTask SaveAsync(IEnumerable<BookmarkEntity> entities, CancellationToken cancellationToken = default);
}
