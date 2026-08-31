using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.ActivitySchedulers.Models;

public class ActivityWorkItem
{
    public ActivityWorkItem(
        IActivity activity,
        ActivityExecutionContext? owner = null,
        IEnumerable<Variable>? variables = null,
        ActivityExecutionContext? existingActivityExecutionContext = null,
        IDictionary<string, object>? input = null,
        string? schedulingActivityExecutionId = null,
        string? schedulingWorkflowInstanceId = null,
        int? schedulingCallStackDepth = null)
    {
        Activity = activity;
        Owner = owner;
        Variables = variables;
        ExistingActivityExecutionContext = existingActivityExecutionContext;
        Input = input ?? new Dictionary<string, object>();
        SchedulingActivityExecutionId = schedulingActivityExecutionId;
        SchedulingWorkflowInstanceId = schedulingWorkflowInstanceId;
        SchedulingCallStackDepth = schedulingCallStackDepth;
    }

    public IActivity Activity { get; }

    public ActivityExecutionContext? Owner { get; set; }

    public IEnumerable<Variable>? Variables { get; set; }

    public ActivityExecutionContext? ExistingActivityExecutionContext { get; set; }

    public IDictionary<string, object> Input { get; set; }

    public string? SchedulingActivityExecutionId { get; set; }

    public string? SchedulingWorkflowInstanceId { get; set; }

    public int? SchedulingCallStackDepth { get; set; }
}
