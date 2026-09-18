using Cike.Core.DependencyInjection;
using Cike.EventBus;
using Cike.EventBus.Local;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Contexts.Models;
using Cike.Workflow.Runtime.Bookmarks.Models;
using Elsa.Workflows.Runtime.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Runtime.Bookmarks;

internal class BookmarkManager(IBookmarkRepository bookmarkRepository, IQueueEventBus queueEventBus) : IScopedDependency
{
    /// <inheritdoc />
    public async Task UpdateBookmarksAsync(UpdateBookmarksRequest request, CancellationToken cancellationToken = default)
    {
        var instanceId = request.WorkflowExecutionContext.Id;
        await RemoveBookmarksAsync(instanceId, request.Diff.Removed.ToList(), cancellationToken);
        await StoreBookmarksAsync(request.WorkflowExecutionContext, request.Diff.Added.ToList(), cancellationToken);

        // 发送事件
        await queueEventBus.EnqueueAsync(new WorkflowBookmarksIndexedEvent(
            request.WorkflowExecutionContext,
            request.Diff.Added,
            request.Diff.Removed,
            request.Diff.Unchanged));
    }

    private async Task RemoveBookmarksAsync(long workflowInstanceId, ICollection<Bookmark> bookmarks, CancellationToken cancellationToken)
    {
        if (bookmarks.Count == 0)
            return;

        var matchingIds = bookmarks.Select(x => x.Id).ToList();
        var filter = new BookmarkFilter
        {
            BookmarkIds = matchingIds,
            WorkflowInstanceId = workflowInstanceId
        };
        await bookmarkRepository.DeleteManyAsync(filter, cancellationToken);
    }

    private async Task StoreBookmarksAsync(WorkflowExecutionContext context, ICollection<Bookmark> bookmarks, CancellationToken cancellationToken)
    {
        if (bookmarks.Count == 0)
            return;

        await bookmarkRepository.SaveAsync(bookmarks.Select(e => MapBookmark(context, e)), cancellationToken);
    }

    public BookmarkEntity MapBookmark(WorkflowExecutionContext workflowExecutionContext, Bookmark bookmark)
    {
        return new BookmarkEntity
        {
            Id = bookmark.Id,
            Name = bookmark.Name,
            Hash = bookmark.Hash,
            WorkflowInstanceId = workflowExecutionContext.Id,
            CreatedAt = bookmark.CreatedAt,
            ActivityInstanceId = bookmark.ActivityInstanceId ?? 0,
            CorrelationId = workflowExecutionContext.CorrelationId,
            Payload = bookmark.Payload,
            Metadata = bookmark.Metadata
        };
    }
}
