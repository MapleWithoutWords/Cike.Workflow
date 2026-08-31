using Cike.Workflow.Core.ActivityDescriptors;
using Cike.Workflow.Core.ActivityDescriptors.Internals;
using Cike.Workflow.Core.ActivitySchedulers;

namespace Cike.Workflow.Core.Contexts;

public class WorkflowExecutionContext : IExecutionContext
{
    private ICollection<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();
    private ICollection<CancellationTokenRegistration> _cancellationRegistrations = new List<CancellationTokenRegistration>();
    private IList<ActivityExecutionContext> _activityExecutionContexts;

    private WorkflowExecutionContext(
        IServiceProvider serviceProvider,
        WorkflowGraph workflowGraph,
        long id,
        string correlationId,
        long? parentWorkflowInstanceId,
        IDictionary<string, object>? input,
        IDictionary<string, object>? properties,
        ExecuteActivityDelegate? executeDelegate,
        string? triggerActivityId,
        IEnumerable<Bookmark> originalBookmarks,
        DateTime createdAt,
        CancellationToken cancellationToken)
    {
        ServiceProvider = serviceProvider;
        ActivityRegistry = serviceProvider.GetRequiredService<IActivityRegistry>();
        ActivityRegistryLookup = serviceProvider.GetRequiredService<IActivityRegistryLookupService>();
        Status = WorkflowStatus.Pending;
        Id = id;
        CorrelationId = correlationId;
        ParentWorkflowInstanceId = parentWorkflowInstanceId;
        _activityExecutionContexts = new List<ActivityExecutionContext>();
        Scheduler = serviceProvider.GetRequiredService<IActivitySchedulerFactory>().CreateScheduler();
        Input = input != null ? new(input, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        Properties = properties != null ? new(properties, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        ExecuteDelegate = executeDelegate;
        TriggerActivityId = triggerActivityId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        CancellationToken = cancellationToken;
        OriginalBookmarks = originalBookmarks.ToList();
        WorkflowGraph = workflowGraph;
        var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _cancellationTokenSources.Add(linkedCancellationTokenSource);
        _cancellationRegistrations.Add(linkedCancellationTokenSource.Token.Register(CancelWorkflow));
    }

    public long Id { get; set; }

    public long? ParentWorkflowInstanceId { get; set; }

    public string? TriggerActivityId { get; set; }

    public string CorrelationId { get; set; }

    public IDictionary<string, object> Properties { get; set; } = null!;

    public IDictionary<string, object> Input { get; set; } = null!;

    public IDictionary<string, object> Output { get; set; } = new Dictionary<string, object>();

    public WorkflowStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public ExecuteActivityDelegate? ExecuteDelegate { get; set; }

    public ExpressionExecutionContext ExpressionExecutionContext { get; private set; } = null!;

    public MemoryRegister MemoryRegister { get; private set; } = null!;

    public WorkflowGraph WorkflowGraph { get; private set; } = null!;

    public CancellationToken CancellationToken { get; }

    public IActivityScheduler Scheduler { get; }

    public IActivityRegistry ActivityRegistry { get; }

    public IActivityRegistryLookupService ActivityRegistryLookup { get; }

    public IActivity Activity => WorkflowGraph.Workflow;

    public IEnumerable<Variable> Variables => WorkflowGraph.Workflow.Variables;

    /// A graph of the workflow structure.
    public ActivityNode Graph => WorkflowGraph.Root;

    public IServiceProvider ServiceProvider { get; } = null!;

    public T GetRequiredService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

    public object GetRequiredService(Type serviceType) => ServiceProvider.GetRequiredService(serviceType);
}
