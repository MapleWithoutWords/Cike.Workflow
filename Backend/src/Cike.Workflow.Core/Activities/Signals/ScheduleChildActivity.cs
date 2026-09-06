namespace Cike.Workflow.Core.Activities.Signals;

public class ScheduleChildActivitySignal
{
    public ScheduleChildActivitySignal(IActivity activity, IDictionary<string, object>? input = default)
    {
        Activity = activity;
        Input = input;
    }

    public ScheduleChildActivitySignal(ActivityExecutionContext? activityExecutionContext, IDictionary<string, object>? input = default)
    {
        ActivityExecutionContext = activityExecutionContext;
        Input = input;
    }

    public IActivity? Activity { get; init; }

    public IDictionary<string, object>? Input { get; set; }

    public ActivityExecutionContext? ActivityExecutionContext { get; init; }
}
