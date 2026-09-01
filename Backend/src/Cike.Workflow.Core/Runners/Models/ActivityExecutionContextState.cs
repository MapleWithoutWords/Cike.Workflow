using Cike.Workflow.Core.Variables;

namespace Cike.Workflow.Core.Runners.Models;

public class ActivityExecutionContextState
{
    public ActivityExecutionContextState()
    {
    }

    public long Id { get; set; } = default!;

    public int CallStackDepth { get; set; }

    public long? ParentContextId { get; set; }

    public string ScheduledActivityNodeId { get; set; } = default!;

    public string? OwnerActivityNodeId { get; set; }

    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    public IDictionary<string, object>? ActivityState { get; set; }

    public ICollection<Variable> DynamicVariables { get; set; } = new List<Variable>();

    public ActivityStatus Status { get; set; }

    public bool IsExecuting { get; set; }

    public int FaultCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? FinishedAt { get; set; }
}
