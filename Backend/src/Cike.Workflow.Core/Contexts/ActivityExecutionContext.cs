using Microsoft.Extensions.Logging;

namespace Cike.Workflow.Core.Contexts;

public class ActivityExecutionContext : IExecutionContext
{
    private ActivityExecutionContext? _parentActivityExecutionContext;

    public long Id { get; set; }

    public ActivityStatus Status { get; set; }

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

    public IDictionary<object, object> TransientProperties { get; private set; } = new Dictionary<object, object>();

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

    #region Properties
    public T? GetProperty<T>(string key) => Properties.TryGetValue<T?>(key, out var value) ? value : default;

    public T GetProperty<T>(string key, Func<T> defaultValue)
    {
        if (Properties.TryGetValue<T?>(key, out var value))
            return value!;

        value = defaultValue();
        Properties[key] = value!;

        return value!;
    }

    public void SetProperty<T>(string key, T? value) => Properties[key] = value!;

    public T UpdateProperty<T>(string key, Func<T?, T> updater) where T : notnull
    {
        var value = GetProperty<T?>(key);
        value = updater(value);
        Properties[key] = value;
        return value;
    }

    public void RemoveProperty(string key) => Properties.Remove(key);
    #endregion

    #region Parent/Children
    public ActivityExecutionContext? FindParentWithVariableContainer()
    {
        return FindParent(x => x.Activity is IVariableContainer);
    }

    public ActivityExecutionContext? FindParent(Func<ActivityExecutionContext, bool> predicate)
    {
        var currentContext = this;

        while (currentContext != null)
        {
            if (predicate(currentContext))
                return currentContext;

            currentContext = currentContext.ParentActivityExecutionContext;
        }

        return null;
    }

    public IEnumerable<ActivityExecutionContext> GetAncestors()
    {
        var current = ParentActivityExecutionContext;

        while (current != null)
        {
            yield return current;
            current = current.ParentActivityExecutionContext;
        }
    }

    public IEnumerable<ActivityExecutionContext> GetChildren()
    {
        return Children;
    }

    public IEnumerable<ActivityExecutionContext> GetDescendants()
    {
        var children = Children.ToList();

        foreach (var child in children)
        {
            yield return child;

            foreach (var descendant in child.GetDescendants())
                yield return descendant;
        }
    }
    #endregion

    public T GetRequiredService<T>() where T : notnull => WorkflowExecutionContext.GetRequiredService<T>();

    public object GetRequiredService(Type serviceType) => WorkflowExecutionContext.GetRequiredService(serviceType);

    public async ValueTask SendSignalAsync(object signal)
    {
        var receivingContexts = new[]
        {
                this
            }.Concat(this.GetAncestors()).ToList();
        var logger = this.GetRequiredService<ILogger<ActivityExecutionContext>>();
        var signalType = signal.GetType();
        var signalTypeName = signalType.Name;

        // Let all ancestors receive the signal.
        foreach (var ancestorContext in receivingContexts)
        {
            var signalContext = new SignalContext(ancestorContext, this, WorkflowExecutionContext.CancellationToken);

            if (ancestorContext.Activity is not ISignalHandler handler)
                continue;

            logger.LogDebug("Receiving signal {SignalType} on activity {ActivityId} of type {ActivityType}", signalTypeName, ancestorContext.Activity.Id, ancestorContext.Activity.Type);
            await handler.ReceiveSignalAsync(signal, signalContext);

            if (signalContext.StopPropagationRequested)
            {
                logger.LogDebug("Propagation of signal {SignalType} on activity {ActivityId} of type {ActivityType} was stopped", signalTypeName, ancestorContext.Activity.Id, ancestorContext.Activity.Type);
                return;
            }
        }
    }

    public async ValueTask CompleteActivityAsync(Outcomes? outcomes = null)
    {

    }
}
