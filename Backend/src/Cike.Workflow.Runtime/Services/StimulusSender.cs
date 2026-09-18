using Cike.Core.DependencyInjection;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Runtime.Bookmarks.Models;
using Cike.Workflow.Runtime.Triggers;

namespace Cike.Workflow.Runtime.Internals;

public class StimulusSender(
    IStimulusHasher stimulusHasher,
    IWorkflowRuntime workflowRuntime,
    ITriggerBoundWorkflowService triggerBoundWorkflowService,
    IBookmarkTrigger bookmarkTrigger,
    IBookmarkQueue bookmarkQueue,
    ILogger<StimulusSender> logger) : IStimulusSender, IScopedDependency
{
    /// <inheritdoc />
    public Task<SendStimulusResult> SendAsync(string activityTypeName, object stimulus, StimulusMetadata? metadata = null, CancellationToken cancellationToken = default)
    {
        var stimulusHash = stimulusHasher.Hash(activityTypeName, stimulus, metadata?.ActivityInstanceId);
        return SendAsync(stimulusHash, metadata, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SendStimulusResult> SendAsync(string stimulusHash, StimulusMetadata? metadata = null, CancellationToken cancellationToken = default)
    {
        var responses = new List<RunWorkflowInstanceResponse>();

        if (metadata == null || (metadata.WorkflowInstanceId == null && metadata.BookmarkId == null && metadata.ActivityInstanceId == null))
        {
            var triggered = await TriggerNewWorkflowsAsync(stimulusHash!, metadata, cancellationToken);
            responses.AddRange(triggered);
        }

        var resumed = await ResumeExistingWorkflowsAsync(stimulusHash, metadata, cancellationToken);
        responses.AddRange(resumed);
        return new(responses);
    }

    private async Task<ICollection<RunWorkflowInstanceResponse>> TriggerNewWorkflowsAsync(string stimulusHash, StimulusMetadata? metadata = null, CancellationToken cancellationToken = default)
    {
        var triggerBoundWorkflows = await triggerBoundWorkflowService.FindManyAsync(stimulusHash, cancellationToken);
        var correlationId = metadata?.CorrelationId;
        var input = metadata?.Input;
        var properties = metadata?.Properties;
        var parentId = metadata?.ParentWorkflowInstanceId;
        var responses = new List<RunWorkflowInstanceResponse>();

        foreach (var triggerBoundWorkflow in triggerBoundWorkflows)
        {
            var workflowGraph = triggerBoundWorkflow.WorkflowGraph;
            var workflow = workflowGraph.Workflow;

            foreach (var trigger in triggerBoundWorkflow.Triggers)
            {
                var workflowClient = await workflowRuntime.CreateClientAsync(0, cancellationToken);
                var createWorkflowInstanceRequest = new CreateAndRunWorkflowInstanceRequest
                {
                    WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionVersionId(workflow.DefinitionInfo.Id),
                    CorrelationId = correlationId,
                    Name = "",
                    Input = input,
                    TriggerActivityId = trigger.ActivityId,
                    Properties = properties,
                    ParentId = parentId
                };

                var runWorkflowResponse = await workflowClient.CreateAndRunInstanceAsync(createWorkflowInstanceRequest, cancellationToken);

                responses.Add(runWorkflowResponse);
            }
        }

        return responses;
    }

    private async Task<ICollection<RunWorkflowInstanceResponse>> ResumeExistingWorkflowsAsync(string stimulusHash, StimulusMetadata? metadata, CancellationToken cancellationToken)
    {
        var input = metadata?.Input;
        var properties = metadata?.Properties;

        var bookmarkFilter = new BookmarkFilter
        {
            Hash = stimulusHash,
            CorrelationId = metadata?.CorrelationId,
            WorkflowInstanceId = metadata?.WorkflowInstanceId,
            ActivityInstanceId = metadata?.ActivityInstanceId,
            BookmarkId = metadata?.BookmarkId
        };
        var responses = (await bookmarkTrigger.ResumeAsync(bookmarkFilter, new()
        {
            Input = input,
            Properties = properties
        }, cancellationToken)).ToList();

        if (responses.Count > 0)
        {
            logger.LogDebug("Successfully resumed {WorkflowCount} workflow instances using stimulus {StimulusHash}", responses.Count, stimulusHash);
            return responses;
        }

        // If no bookmarks were matched, enqueue the request in case a matching bookmark is created in the near future.
        var workflowInstanceId = metadata?.WorkflowInstanceId;

        logger.LogDebug("Bookmark queue item enqueued with stimulus: {StimulusHash}", stimulusHash);

        await bookmarkQueue.EnqueueAsync(workflowInstanceId ?? 0, metadata?.BookmarkId ?? 0, metadata?.CorrelationId ?? "", stimulusHash, metadata?.ActivityInstanceId ?? 0, new()
        {
            Input = input,
            Properties = properties
        }, cancellationToken);

        return responses;
    }
}
