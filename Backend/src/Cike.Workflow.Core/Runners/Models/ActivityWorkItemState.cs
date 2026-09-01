namespace Cike.Workflow.Core.Runners.Models;

public class ActivityWorkItemState
{
    public string ActivityNodeId { get; set; } = default!;

    public long? OwnerContextId { get; set; }

    public ICollection<Variable>? Variables { get; set; }

    public long? ExistingActivityExecutionContextId { get; set; }

    public IDictionary<string, object> Input { get; set; } = new Dictionary<string, object>();
}
