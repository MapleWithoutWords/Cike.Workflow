using Cike.Workflow.Core.Serialization;
using Flurl.Http.Configuration;

namespace Cike.EntityFrameworkCore.Stores;

public class BookmarkStore(CikeWorkflowDbContenxt context, IPayloadSerializer payloadSerializer)
    : BaseStore<BookmarkEntity>(context), IBookmarkStore
{
    public override async Task AddRangeAsync(IEnumerable<BookmarkEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var item in entities)
        {
            await OnSaveAsync(item, cancellationToken);
        }
        await base.AddRangeAsync(entities, cancellationToken);
    }

    public override async Task<BookmarkEntity?> FindAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await base.FindAsync(id, cancellationToken);
        await OnLoadAsync(result, cancellationToken);
        return result;
    }

    public override async Task<List<BookmarkEntity>> GetListAsync(Expression<Func<BookmarkEntity, bool>> filter, string sorting = "CreatedAt desc", CancellationToken cancellationToken = default)
    {
        var result = await base.GetListAsync(filter, sorting, cancellationToken);
        foreach (var item in result)
        {
            await OnLoadAsync(item, cancellationToken);
        }
        return result;
    }

    private ValueTask OnSaveAsync(BookmarkEntity entity, CancellationToken cancellationToken)
    {
        context.Entry(entity).Property("SerializedPayload").CurrentValue = entity.Payload != null ? payloadSerializer.Serialize(entity.Payload) : null;
        context.Entry(entity).Property("SerializedMetadata").CurrentValue = entity.Metadata != null ? payloadSerializer.Serialize(entity.Metadata) : null;
        return default;
    }

    private ValueTask OnLoadAsync(BookmarkEntity? entity, CancellationToken cancellationToken)
    {
        if (entity is null)
            return default;

        var payloadJson = context.Entry(entity).Property<string>("SerializedPayload").CurrentValue;
        var metadataJson = context.Entry(entity).Property<string>("SerializedMetadata").CurrentValue;
        entity.Payload = !string.IsNullOrEmpty(payloadJson) ? payloadSerializer.Deserialize(payloadJson) : null;
        entity.Metadata = !string.IsNullOrEmpty(metadataJson) ? payloadSerializer.Deserialize<Dictionary<string, string>>(metadataJson) : null;

        return default;
    }
}
