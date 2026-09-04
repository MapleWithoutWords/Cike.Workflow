namespace Cike.Workflow.Core.Activities.Behaviors;

/// <summary>
/// Implements a behavior that invokes "child completed" callbacks on parent activities.
/// </summary>
public class ScheduledChildCallbackBehavior : Behavior
{
    /// <inheritdoc />
    public ScheduledChildCallbackBehavior(IActivity owner) : base(owner)
    {
        OnSignalReceived<ActivityCompletedSignal>(OnActivityCompletedAsync);
    }

    private async ValueTask OnActivityCompletedAsync(ActivityCompletedSignal signal, SignalContext context)
    {
        var activityExecutionContext = context.ReceiverActivityExecutionContext;
        var childActivityExecutionContext = context.SenderActivityExecutionContext;
        var childActivityNode = childActivityExecutionContext.ActivityNode;
        var callbackEntry = activityExecutionContext.WorkflowExecutionContext.PopCompletionCallback(activityExecutionContext, childActivityNode);

        if (callbackEntry?.CompletionCallback != null)
        {
            var completedContext = new ActivityCompletedContext(activityExecutionContext, childActivityExecutionContext, signal.Result);

            await callbackEntry.CompletionCallback(completedContext);
        }
    }
}
