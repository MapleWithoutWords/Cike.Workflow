namespace Cike.Workflow.Core.Contexts.Models;

public class ActivityInvocationOptions
{
    public ActivityInvocationOptions()
    {
        Input = new Dictionary<string, object>();
    }

    public ActivityInvocationOptions(
        ActivityExecutionContext? owner,
        IEnumerable<Variable>? variables,
        ActivityExecutionContext? existingActivityExecutionContext = default,
        IDictionary<string, object>? input = default)
    {
        Owner = owner;
        Variables = variables;
        ExistingActivityExecutionContext = existingActivityExecutionContext;
        Input = input ?? new Dictionary<string, object>();
    }

    public ActivityExecutionContext? Owner { get; set; }

    public IEnumerable<Variable>? Variables { get; set; }

    public ActivityExecutionContext? ExistingActivityExecutionContext { get; set; }

    public IDictionary<string, object> Input { get; set; }

    public long? SchedulingActivityExecutionId { get; set; }

    public long? SchedulingWorkflowInstanceId { get; set; }

    public int? SchedulingCallStackDepth { get; set; }
}
