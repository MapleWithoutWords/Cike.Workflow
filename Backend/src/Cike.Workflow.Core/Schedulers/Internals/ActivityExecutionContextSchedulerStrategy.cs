using Cike.Workflow.Core.Schedulers;
using Cike.Workflow.Core.Schedulers.Models;
using Cike.Workflow.Core.WorkflowGraphs.Models;

namespace Cike.Workflow.Core.Schedulers.Internals;

/// <inheritdoc />
public class ActivityExecutionContextSchedulerStrategy : IActivityExecutionContextSchedulerStrategy, ISingletonDependency
{
    /// <inheritdoc />
    public async Task ScheduleActivityAsync(ActivityExecutionContext context, IActivity? activity, ActivityExecutionContext? owner, ScheduleWorkOptions? options = null)
    {
        var activityNode = activity != null
            ? context.WorkflowExecutionContext.FindNodeByActivity(activity) ?? throw new InvalidOperationException("The specified activity is not part of the workflow.")
            : null;
        await ScheduleActivityAsync(context, activityNode, owner, options);
    }

    /// <inheritdoc />
    public async Task ScheduleActivityAsync(ActivityExecutionContext context, ActivityNode? activityNode, ActivityExecutionContext? owner = null, ScheduleWorkOptions? options = null)
    {
        var workflowExecutionContext = context.WorkflowExecutionContext;

        var completionCallback = options?.CompletionCallback;
        owner ??= context;

        if (activityNode == null)
        {
            if (completionCallback != null)
            {
                var completedContext = new ActivityCompletedContext(context, context);
                await completionCallback(completedContext);
            }
            else
                await owner.CompleteActivityAsync();

            return;
        }

        workflowExecutionContext.Schedule(activityNode, owner, options);
    }
}
