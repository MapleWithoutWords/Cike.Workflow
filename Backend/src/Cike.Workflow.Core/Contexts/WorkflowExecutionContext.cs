namespace Cike.Workflow.Core.Contexts;

public class WorkflowExecutionContext
{
    public long Id { get; set; }

    public IActivity Activity => WorkflowGraph.Workflow;

    public IEnumerable<Variable> Variables => WorkflowGraph.Workflow.Variables;

    public IDictionary<string, object> Properties { get; set; }

    public IDictionary<string, object> Input { get; set; }

    public IDictionary<string, object> Output { get; set; } = new Dictionary<string, object>();

    public WorkflowGraph WorkflowGraph { get; private set; } = null!;

    /// A graph of the workflow structure.
    public ActivityNode Graph => WorkflowGraph.Root;
}
