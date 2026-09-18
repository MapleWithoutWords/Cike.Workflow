using Cike.Core.DependencyInjection;

namespace Cike.Workflow.Runtime.Bookmarks.Internals;

public class BookmarkQueue(IBookmarkQueueItemRepository bookmarkQueueItemRepository, IUnitOfWork unitOfWork) : IBookmarkQueue, IScopedDependency
{
    public async Task EnqueueAsync(long workflowInstanceId, long bookmarkId, string correlationId, string stimulusHash, long activityInstanceId, ResumeBookmarkOptionsValueObject options, CancellationToken cancellationToken = default)
    {
        await bookmarkQueueItemRepository.InsertAsync(new BookmarkQueueItem
        {
            WorkflowInstanceId = workflowInstanceId,
            BookmarkId = bookmarkId,
            CorrelationId = correlationId,
            StimulusHash = stimulusHash,
            ActivityInstanceId = activityInstanceId,
            Options = options,
        }, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
