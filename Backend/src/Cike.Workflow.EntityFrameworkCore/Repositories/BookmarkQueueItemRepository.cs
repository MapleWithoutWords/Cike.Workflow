using Cike.EntityFrameworkCore.Repositories;
using Cike.Workflow.Core.Serialization;
using Cike.Workflow.Domain.Shared.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.EntityFrameworkCore.Repositories;

internal class BookmarkQueueItemRepository(CikeWorkflowDbContext context, IPayloadSerializer payloadSerializer) : SerializedEfCoreRepository<CikeWorkflowDbContext, BookmarkQueueItem>(context), IBookmarkQueueItemRepository, IScopedDependency
{
    public override async Task<List<BookmarkQueueItem>> ToListAsync(IQueryable<BookmarkQueueItem> query, CancellationToken cancellationToken = default)
    {
        var result = await base.ToListAsync(query, cancellationToken);
        foreach (var item in result)
        {
            await OnLoadAsync(item, cancellationToken);
        }
        return result;
    }

    public override async Task<(long Total, List<BookmarkQueueItem> Items)> ToPagedListAsync(IQueryable<BookmarkQueueItem> query, IPagedAndSortedRequest request, CancellationToken cancellationToken = default)
    {
        var pageResult = await base.ToPagedListAsync(query, request, cancellationToken);
        foreach (var item in pageResult.Items)
        {
            await OnLoadAsync(item, cancellationToken);
        }
        return pageResult;
    }

    protected override ValueTask OnSaveAsync(BookmarkQueueItem entity, CancellationToken cancellationToken)
    {
        DbContext.Entry(entity).Property("SerializedOptions").CurrentValue = entity.Options != null ? payloadSerializer.Serialize(entity.Options) : null;
        return default;
    }

    protected override ValueTask OnLoadAsync(BookmarkQueueItem? entity, CancellationToken cancellationToken)
    {
        if (entity is null)
            return default;

        var optionsJson = DbContext.Entry(entity).Property<string>("SerializedOptions").CurrentValue;
        entity.Options = !string.IsNullOrEmpty(optionsJson) ? payloadSerializer.Deserialize<ResumeBookmarkOptionsValueObject>(optionsJson) : null;

        return default;
    }
}
