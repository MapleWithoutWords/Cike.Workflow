namespace Cike.Workflow.Core.Activities;

/// <summary>
/// Execute a set of activities in sequence.
/// </summary>
[Category("Workflows")]
[Activity("Cike", "Workflows", "Execute a set of activities in sequence.")]
[Browsable(false)]
public class Sequence : ContainerActivity
{
    private const string CurrentIndexProperty = "CurrentIndex";

    /// <inheritdoc />
    public Sequence() : base()
    {
        OnSignalReceived<BreakSignal>(OnBreakSignalReceived);
    }

    /// <inheritdoc />
    protected override async ValueTask ScheduleChildrenAsync(ActivityExecutionContext context)
    {
        await HandleItemAsync(context);
    }

    private async ValueTask HandleItemAsync(ActivityExecutionContext context, ActivityExecutionContext? completedChildContext = null)
    {
        var currentIndex = context.GetProperty<int>(CurrentIndexProperty);
        var childActivities = Activities.ToList();

        if (currentIndex >= childActivities.Count)
        {
            await context.CompleteActivityAsync();
            return;
        }

        var nextActivity = childActivities.ElementAt(currentIndex);
        var options = new ScheduleWorkOptions
        {
            CompletionCallback = OnChildCompleted,
            SchedulingActivityExecutionId = completedChildContext?.Id
        };
        await context.ScheduleActivityAsync(nextActivity, options);
        context.UpdateProperty<int>(CurrentIndexProperty, x => x + 1);
    }

    private async ValueTask OnChildCompleted(ActivityCompletedContext context)
    {
        var targetContext = context.TargetContext;
        var childContext = context.ChildContext;
        var isBreaking = targetContext.GetIsBreakingProperty();
        var completedActivity = childContext.Activity;

        // If the complete activity is a terminal node, complete the sequence immediately.
        if (isBreaking || completedActivity is ITerminalNode)
        {
            await targetContext.CompleteActivityAsync();
            return;
        }

        await HandleItemAsync(targetContext, childContext);
    }

    private void OnBreakSignalReceived(BreakSignal signal, SignalContext signalContext)
    {
        signalContext.ReceiverActivityExecutionContext.SetIsBreakingProperty();
    }
}
