using System.Linq.Expressions;

namespace Cike.Workflow.Domain.Data;

public interface IBookmarkRepository : IRepository<BookmarkEntity, long>
{
    /// <summary>
    /// 带排序的列表查询（兼容迁移前的 Store 签名，排序经 System.Linq.Dynamic.Core 解析），结果含影子属性反序列化。
    /// </summary>
    Task<List<BookmarkEntity>> GetListAsync(Expression<Func<BookmarkEntity, bool>> filter, string sorting = "CreatedAt desc", CancellationToken cancellationToken = default);
}
