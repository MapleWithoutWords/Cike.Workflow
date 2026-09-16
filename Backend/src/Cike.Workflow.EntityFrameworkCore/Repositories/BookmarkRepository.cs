using Cike.Workflow.Core.Serialization;
using System.Linq.Dynamic.Core;

namespace Cike.EntityFrameworkCore.Repositories;

public class BookmarkRepository(CikeWorkflowDbContext context, IPayloadSerializer payloadSerializer)
    : SerializedEfCoreRepository<CikeWorkflowDbContext, BookmarkEntity>(context), IBookmarkRepository, IScopedDependency
{
    /// <summary>框架的无排序列表查询与迁移前的列表行为对齐：结果同样反序列化。</summary>
    public override Task<List<BookmarkEntity>> GetListAsync(Expression<Func<BookmarkEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => GetListAsync(predicate, "CreatedAt desc", cancellationToken);

    public async Task<List<BookmarkEntity>> GetListAsync(Expression<Func<BookmarkEntity, bool>> filter, string sorting = "CreatedAt desc", CancellationToken cancellationToken = default)
    {
        // 跟踪查询：无跟踪实体不携带影子属性值，Entry(...).CurrentValue 只会读到 null
        var result = await GetQueryable().Where(filter).OrderBy(sorting).ToListAsync(cancellationToken);
        foreach (var item in result)
        {
            await OnLoadAsync(item, cancellationToken);
        }
        return result;
    }

    protected override ValueTask OnSaveAsync(BookmarkEntity entity, CancellationToken cancellationToken)
    {
        DbContext.Entry(entity).Property("SerializedPayload").CurrentValue = entity.Payload != null ? payloadSerializer.Serialize(entity.Payload) : null;
        DbContext.Entry(entity).Property("SerializedMetadata").CurrentValue = entity.Metadata != null ? payloadSerializer.Serialize(entity.Metadata) : null;
        return default;
    }

    protected override ValueTask OnLoadAsync(BookmarkEntity? entity, CancellationToken cancellationToken)
    {
        if (entity is null)
            return default;

        var payloadJson = DbContext.Entry(entity).Property<string>("SerializedPayload").CurrentValue;
        var metadataJson = DbContext.Entry(entity).Property<string>("SerializedMetadata").CurrentValue;
        entity.Payload = !string.IsNullOrEmpty(payloadJson) ? payloadSerializer.Deserialize(payloadJson) : null;
        entity.Metadata = !string.IsNullOrEmpty(metadataJson) ? payloadSerializer.Deserialize<Dictionary<string, string>>(metadataJson) : null;

        return default;
    }
}
