namespace Cike.Workflow.Core.Activities;

[Activity("Cike", "Workflows", "Execute a set of activities in parallel.")]
[Browsable(false)]
public class Parallel : ContainerActivity
{
    private const string ScheduledChildrenProperty = "ScheduledChildren";

    /// <inheritdoc />
    public Parallel() : base()
    {
    }

    /// <inheritdoc />
    public Parallel(params IActivity[] activities) : this()
    {
        this.Activities = activities;
    }

    /// <inheritdoc />
    protected override async ValueTask ScheduleChildrenAsync(ActivityExecutionContext context)
    {
        // If there are no activities, complete immediately
        if (Activities.Count == 0)
        {
            await context.CompleteActivityAsync();
            return;
        }

        context.SetProperty(ScheduledChildrenProperty, Activities.Count);

        // For Parallel, all children are scheduled by the parent activity (this), so the scheduling activity is the Parallel itself
        var options = new ScheduleWorkOptions
        {
            CompletionCallback = OnChildCompleted,
            SchedulingActivityExecutionId = context.Id
        };

        foreach (var activity in Activities)
            await context.ScheduleActivityAsync(activity, options);
    }

    private static async ValueTask OnChildCompleted(ActivityCompletedContext context)
    {
        var remainingChildren = context.TargetContext.UpdateProperty<int>(ScheduledChildrenProperty, scheduledChildren => scheduledChildren - 1);

        if (remainingChildren == 0)
            await context.TargetContext.CompleteActivityAsync();
    }
}
