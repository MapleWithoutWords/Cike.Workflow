using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Contexts;

public class WorkflowExecutionContext
{
    public long Id { get; set; }

    public IDictionary<string, object> Properties { get; set; } = null!;

    public IDictionary<string, object> Input { get; set; } = null!;

    public IDictionary<string, object> Output { get; set; } = new Dictionary<string, object>();

    public MemoryRegister MemoryRegister { get; private set; } = null!;

    public WorkflowGraph WorkflowGraph { get; private set; } = null!;

    public CancellationToken CancellationToken { get; }

    public IActivity Activity => WorkflowGraph.Workflow;

    public IEnumerable<Variable> Variables => WorkflowGraph.Workflow.Variables;

    /// A graph of the workflow structure.
    public ActivityNode Graph => WorkflowGraph.Root;

    public IServiceProvider ServiceProvider { get; } = null!;

    public T GetRequiredService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

    public object GetRequiredService(Type serviceType) => ServiceProvider.GetRequiredService(serviceType);
}
