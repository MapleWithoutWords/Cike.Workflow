using Cike.Core.DependencyInjection;
using Cike.Locks.Abstracts;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Helpers;
using Cike.Workflow.Runtime.Bookmarks.Models;
using Cike.Workflow.Runtime.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cike.Workflow.Runtime.Bookmarks.Internals;

/// <inheritdoc />
public class BookmarkTrigger(
    IWorkflowRuntime workflowRuntime,
    IBookmarkRepository bookmarkStore,
    IStimulusHasher stimulusHasher,
    ILock distributedLockProvider,
    ILogger<BookmarkTrigger> logger) : IBookmarkTrigger, IScopedDependency
{
    /// <inheritdoc />
    public Task<IEnumerable<RunWorkflowInstanceResponse>> ResumeAsync<TActivity>(object stimulus, ResumeBookmarkOptions? options = null, CancellationToken cancellationToken = default) where TActivity : IActivity
    {
        return ResumeAsync<TActivity>(stimulus, null, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RunWorkflowInstanceResponse>> ResumeAsync<TActivity>(object stimulus, long? workflowInstanceId = null, ResumeBookmarkOptions? options = null, CancellationToken cancellationToken = default) where TActivity : IActivity
    {
        var activityTypeName = ActivityTypeNameHelper.GenerateTypeName<TActivity>();
        var stimulusHash = stimulusHasher.Hash(activityTypeName, stimulus);
        var bookmarkFilter = new BookmarkFilter
        {
            Name = activityTypeName,
            WorkflowInstanceId = workflowInstanceId,
            Hash = stimulusHash,
        };
        return await ResumeAsync(bookmarkFilter, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RunWorkflowInstanceResponse?> ResumeAsync(long bookmarkId, IDictionary<string, object> input, CancellationToken cancellationToken = default)
    {
        var bookmarkFilter = new BookmarkFilter
        {
            BookmarkId = bookmarkId
        };
        var options = new ResumeBookmarkOptions
        {
            Input = input
        };
        var responses = await ResumeAsync(bookmarkFilter, options, cancellationToken);
        return responses.FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<RunWorkflowInstanceResponse?> ResumeAsync<TActivity>(long bookmarkId, ResumeBookmarkOptions? options = null, CancellationToken cancellationToken = default) where TActivity : IActivity
    {
        var activityTypeName = ActivityTypeNameHelper.GenerateTypeName<TActivity>();
        var bookmarkFilter = new BookmarkFilter
        {
            Name = activityTypeName,
            BookmarkId = bookmarkId
        };
        var response = await ResumeAsync(bookmarkFilter, options, cancellationToken);
        return response.FirstOrDefault();
    }

    public async Task<IEnumerable<RunWorkflowInstanceResponse>> ResumeAsync(ResumeBookmarkRequest request, CancellationToken cancellationToken = default)
    {
        var filter = new BookmarkFilter
        {
            BookmarkId = request.BookmarkId,
            ActivityInstanceId = request.ActivityInstanceId,
        };

        var resumeOptions = new ResumeBookmarkOptions()
        {
            Input = request.Input,
            Properties = request.Properties,
        };
        return await ResumeAsync(filter, resumeOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RunWorkflowInstanceResponse>> ResumeAsync(BookmarkFilter filter, ResumeBookmarkOptions? options = null, CancellationToken cancellationToken = default)
    {
        var hashableFilterString = filter.GetHashableString();
        var lockKey = $"workflow-resumer:{hashableFilterString}";

        try
        {
            await using var filterLock = await distributedLockProvider.TryGetAsync(lockKey, TimeSpan.FromMinutes(10), cancellationToken);
            var bookmarks = await bookmarkStore.FindManyAsync(filter, cancellationToken);

            if (bookmarks.Count() == 0)
            {
                logger.LogDebug("No bookmarks found in store for filter {@Filter}", filter);
                return [];
            }

            var responses = new List<RunWorkflowInstanceResponse>();
            foreach (var bookmark in bookmarks)
            {
                var workflowClient = await workflowRuntime.CreateClientAsync(bookmark.WorkflowInstanceId, cancellationToken);
                var runRequest = new RunWorkflowInstanceRequest
                {
                    Input = options?.Input,
                    Properties = options?.Properties,
                    BookmarkId = bookmark.Id
                };

                try
                {
                    var response = await workflowClient.RunInstanceAsync(runRequest, cancellationToken);
                    logger.LogDebug("Resumed workflow instance {WorkflowInstanceId} with bookmark {BookmarkId}", bookmark.WorkflowInstanceId, bookmark.Id);
                    responses.Add(response);
                }
                catch (WorkflowInstanceNotFoundException)
                {
                    // The workflow instance does not (yet) exist in the DB.
                    logger.LogDebug("No workflow instance with ID {WorkflowInstanceId} found for bookmark {BookmarkId} at this time.", bookmark.WorkflowInstanceId, bookmark.Id);
                }
            }

            return responses;
        }
        catch (TimeoutException e)
        {
            // Rethrow but with a more specific message.
            throw new TimeoutException($"Could not acquire distributed lock with key '{lockKey}' within the configured timeout of {TimeSpan.FromMinutes(10)}.", e);
        }
    }
}
