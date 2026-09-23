using Cike.Workflow.Core.Contexts.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Cike.Workflow.Core.Runners.Internals;

internal class WorkflowInstanceRunHandler(ILocalEventBus localEventBus, ILogger<WorkflowInstanceRunHandler> logger)
{
    [LocalEventHandler]
    public async Task RunWorkflowInstanceAsync(RunWorkflowInstanceCommand command, CancellationToken cancellationToken)
    {
        var context = command.Context;
        var scheduler = context.Scheduler;

        context.TransitionTo(WorkflowStatus.Executing);
        //await ConditionallyCommitStateAsync(context, WorkflowLifetimeEvent.WorkflowExecuting);

        while (scheduler.HasAny)
        {
            // Do not start a workflow if cancellation has been requested.
            if (context.CancellationToken.IsCancellationRequested)
                break;

            var currentWorkItem = scheduler.Take();
            await ExecuteWorkItemAsync(context, currentWorkItem, cancellationToken);
        }

        if (context.Status.GetMainStatus() == WorkflowMainStatus.Running)
            context.TransitionTo(context.ActivityExecutionContexts.All(x => x.IsCompleted) ? WorkflowStatus.Finished : WorkflowStatus.Suspended);
    }

    [LocalEventHandler]
    public async Task RunActivityInstanceAsync(RunActivityInstanceCommand command, CancellationToken cancellationToken)
    {
        var context = command.Context;
        context.CancellationToken.ThrowIfCancellationRequested();

        var workflowExecutionContext = context.WorkflowExecutionContext;

        // Evaluate input properties.
        await context.EvaluateInputPropertiesAsync();

        // Prevent the activity from being started if cancellation is requested.
        if (context.CancellationToken.IsCancellationRequested)
        {
            context.TransitionTo(ActivityStatus.Canceled);
            context.AddExecutionLogEntry("Activity cancelled");
            return;
        }

        // Check if the activity can be executed.
        if (!await context.Activity.CanExecuteAsync(context))
        {
            context.TransitionTo(ActivityStatus.Pending);
            context.AddExecutionLogEntry("Precondition Failed", "Cannot execute at this time");
            return;
        }

        // Mark workflow and activity as executing.
        using var executionState = context.EnterExecution();

        var previousActivityStatus = context.Status;
        context.TransitionTo(ActivityStatus.Running);

        // Execute activity.
        await ExecuteActivityAsync(context);

        var currentActivityStatus = context.Status;
        var activityDidComplete = previousActivityStatus != ActivityStatus.Completed && currentActivityStatus == ActivityStatus.Completed;

        // Reset execute delegate.
        workflowExecutionContext.ExecuteDelegate = null;

        // If a bookmark was used to resume, burn it if not burnt already by the activity.
        var resumedBookmark = workflowExecutionContext.ResumedBookmarkContext?.Bookmark;

        if (resumedBookmark is { AutoBurn: true })
        {
            logger.LogDebug("Auto-burning bookmark {BookmarkId}", resumedBookmark.Id);
            workflowExecutionContext.Bookmarks.Remove(resumedBookmark);
        }

        // Conditionally commit the workflow state.
    }

    protected virtual async ValueTask ExecuteActivityAsync(ActivityExecutionContext context)
    {
        if (context.WorkflowExecutionContext.ExecuteDelegate != null)
        {
            await context.WorkflowExecutionContext.ExecuteDelegate(context);
        }
        else
        {
            await context.Activity.ExecuteAsync(context);
        }
    }

    private async Task<ActivityExecutionContext> ExecuteWorkItemAsync(WorkflowExecutionContext context, ActivityWorkItem workItem, CancellationToken cancellationToken)
    {
        // Setup an activity execution context, potentially reusing an existing one if requested.
        var existingActivityExecutionContext = workItem.ExistingActivityExecutionContext;

        // Perform a lookup to make sure the activity execution context is part of the workflow execution context.
        var activityExecutionContext = existingActivityExecutionContext != null
            ? context.ActivityExecutionContexts.FirstOrDefault(x => x.Id == existingActivityExecutionContext.Id)
            : null;

        if (activityExecutionContext == null)
        {
            // Create a new activity execution context.
            activityExecutionContext = await context.CreateActivityExecutionContextAsync(workItem.Activity, new ActivityInvocationOptions
            {
                Owner = workItem.Owner,
                ExistingActivityExecutionContext = workItem.ExistingActivityExecutionContext,
                Variables = workItem.Variables,
                Input = workItem.Input,
                SchedulingActivityExecutionId = workItem.SchedulingActivityExecutionId,
                SchedulingWorkflowInstanceId = workItem.SchedulingWorkflowInstanceId,
                SchedulingCallStackDepth = workItem.SchedulingCallStackDepth
            });
            activityExecutionContext.Taint();

            // Add the activity context to the workflow context.
            context.AddActivityExecutionContext(activityExecutionContext);
        }

        // Execute the activity execution pipeline.
        await localEventBus.PublishAsync(new RunActivityInstanceCommand(activityExecutionContext), cancellationToken);

        return activityExecutionContext;

    }
}
