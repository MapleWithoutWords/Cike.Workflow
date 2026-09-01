using Cike.Workflow.Core.Variables;

namespace Cike.Workflow.Core.ActivitySchedulers.Models;

public class ScheduleWorkOptions
{
    public ActivityCompletionCallback? CompletionCallback { get; set; }

    public ICollection<Variable>? Variables { get; set; }

    public ActivityExecutionContext? ExistingActivityExecutionContext { get; set; }

    public bool PreventDuplicateScheduling { get; set; }

    public IDictionary<string, object>? Input { get; set; }

    public long? SchedulingActivityExecutionId { get; set; }

    public long? SchedulingWorkflowInstanceId { get; set; }
}
