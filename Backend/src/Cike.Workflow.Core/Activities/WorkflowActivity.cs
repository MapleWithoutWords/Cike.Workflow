namespace Cike.Workflow.Core.Activities;

/// <summary>
/// Represents an executable process.
/// </summary>
[Browsable(false)]
[Activity("Cike", "Workflows", "A workflow is an activity that executes its Root activity.")]
public class WorkflowActivity : CompositeActivity<object>, ICloneable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowActivity"/> class.
    /// </summary>
    public WorkflowActivity(
        IActivity root,
        ICollection<Variable> variables,
        ICollection<InputDefinition> inputs,
        ICollection<OutputDefinition> outputs,
        ICollection<string> outcomes,
        IDictionary<string, object> customProperties,
        bool isReadonly,
        bool isSystem)
    {
        Identity = identity;
        Publication = publication;
        Inputs = inputs;
        Outputs = outputs;
        Outcomes = outcomes;
        WorkflowMetadata = workflowMetadata;
        Options = options;
        Variables = variables;
        CustomProperties = customProperties;
        Root = root;
        IsReadonly = isReadonly;
        IsSystem = isSystem;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowActivity"/> class.
    /// </summary>
    public WorkflowActivity(IActivity root) : this()
    {
        Root = root;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowActivity"/> class.
    /// </summary>
    public WorkflowActivity()
    {
    }

    /// <summary>
    /// Gets or sets input definitions.
    /// </summary>
    public ICollection<InputDefinition> Inputs { get; set; } = new List<InputDefinition>();

    /// <summary>
    /// Gets or sets output definitions.
    /// </summary>
    public ICollection<OutputDefinition> Outputs { get; set; } = new List<OutputDefinition>();

    /// <summary>
    /// Gets or sets possible outcomes for this workflow.
    /// </summary>
    public ICollection<string> Outcomes { get; set; } = new List<string>();

    /// <summary>
    /// Make workflow definition readonly.
    /// </summary>
    public bool IsReadonly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the workflow is a system workflow.
    /// </summary>
    public bool IsSystem { get; }

    /// <summary>
    /// Constructs a new <see cref="WorkflowActivity"/> from the specified <see cref="IActivity"/>.
    /// </summary>
    public static WorkflowActivity FromActivity(IActivity root) => root as WorkflowActivity ?? new(root);

    /// <summary>
    /// Creates a new memory register initialized with this workflow's variables.
    /// </summary>
    public MemoryRegister CreateRegister()
    {
        return new MemoryRegister();
    }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        if (ResultVariable != null)
            context.WorkflowExecutionContext.MemoryRegister.Declare(ResultVariable);

        await base.ExecuteAsync(context);
    }

    /// <summary>
    /// Create a shallow copy of this workflow.
    /// </summary>
    public WorkflowActivity Clone() => (WorkflowActivity)((ICloneable)this).Clone();

    object ICloneable.Clone() => MemberwiseClone();
}
