using Cike.Workflow.Core.Serialization;
using Cike.Workflow.Runtime.Bookmarks.Models;
using Org.BouncyCastle.Ocsp;
using System.Linq.Dynamic.Core;

namespace Cike.EntityFrameworkCore.Repositories;

public class BookmarkRepository(CikeWorkflowDbContext context, IPayloadSerializer payloadSerializer)
    : SerializedEfCoreRepository<CikeWorkflowDbContext, BookmarkEntity>(context), IBookmarkRepository, IScopedDependency
{
    public override async Task<List<BookmarkEntity>> ToListAsync(IQueryable<BookmarkEntity> query, CancellationToken cancellationToken = default)
    {
        var result = await base.ToListAsync(query, cancellationToken);
        foreach (var item in result)
        {
            await OnLoadAsync(item, cancellationToken);
        }
        return result;
    }

    public override async Task<(long Total, List<BookmarkEntity> Items)> ToPagedListAsync(IQueryable<BookmarkEntity> query, IPagedAndSortedRequest request, CancellationToken cancellationToken = default)
    {
        var pageResult = await base.ToPagedListAsync(query, request, cancellationToken);
        foreach (var item in pageResult.Items)
        {
            await OnLoadAsync(item, cancellationToken);
        }
        return pageResult;
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

    public async ValueTask<BookmarkEntity?> FindAsync(BookmarkFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await FindManyAsync(filter, cancellationToken);
        return list.FirstOrDefault();
    }

    public async ValueTask<IEnumerable<BookmarkEntity>> FindManyAsync(BookmarkFilter filter, CancellationToken cancellationToken = default)
    {
        return await ToListAsync(filter.Apply(GetQueryable()), cancellationToken);
    }

    public async ValueTask DeleteManyAsync(BookmarkFilter filter, CancellationToken cancellationToken = default)
    {
        await filter.Apply(GetQueryable()).ExecuteDeleteAsync(cancellationToken);
    }

    public async ValueTask SaveAsync(IEnumerable<BookmarkEntity> entities, CancellationToken cancellationToken = default)
    {
        var exeistBookmarks = await GetQueryable().Where(e => entities.Select(x => x.Id).Contains(e.Id)).Select(e => e.Id).ToListAsync(cancellationToken);
        foreach (var item in entities)
        {
            await OnSaveAsync(item, cancellationToken);
            if (exeistBookmarks.Contains(item.Id))
            {
                context.Bookmarks.Update(item);
            }
            else
            {
                await context.Bookmarks.AddAsync(item, cancellationToken);
            }
        }
        await context.SaveChangesAsync(cancellationToken);
    }
}
