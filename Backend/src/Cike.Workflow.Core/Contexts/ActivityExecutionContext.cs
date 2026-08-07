namespace Cike.Workflow.Core.Contexts;

public class ActivityExecutionContext : IExecutionContext
{
    private ActivityExecutionContext? _parentActivityExecutionContext;

    public long Id { get; set; }

    public IActivity Activity { get; set; } = null!;

    public IEnumerable<Variable> Variables
    {
        get
        {
            var containerVariables = (Activity as IVariableContainer)?.Variables ?? Enumerable.Empty<Variable>();
            var dynamicVariables = DynamicVariables;
            var mergedVariables = new Dictionary<string, Variable>();

            foreach (var containerVariable in containerVariables)
            {
                var name = !string.IsNullOrEmpty(containerVariable.Name) ? containerVariable.Name : containerVariable.Id;
                mergedVariables[name] = containerVariable;
            }

            foreach (var dynamicVariable in dynamicVariables)
            {
                var name = !string.IsNullOrEmpty(dynamicVariable.Name) ? dynamicVariable.Name : dynamicVariable.Id;
                mergedVariables[name] = dynamicVariable;
            }
            return mergedVariables.Values;
        }
    }

    public ICollection<Variable> DynamicVariables { get; set; } = new List<Variable>();

    public IDictionary<string, object> Properties { get; private set; }

    public ExpressionExecutionContext ExpressionExecutionContext { get; } = null!;

    public WorkflowExecutionContext WorkflowExecutionContext { get; } = null!;

    public ActivityExecutionContext? ParentActivityExecutionContext
    {
        get => _parentActivityExecutionContext;
        internal set
        {
            _parentActivityExecutionContext = value;
            _parentActivityExecutionContext?.Children.Add(this);
        }
    }

    public ISet<ActivityExecutionContext> Children { get; } = new HashSet<ActivityExecutionContext>();
}
