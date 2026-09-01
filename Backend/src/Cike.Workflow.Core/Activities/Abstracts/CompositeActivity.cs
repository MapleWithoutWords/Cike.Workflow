using Cike.Workflow.Core.Variables;
using System.Runtime.CompilerServices;

namespace Cike.Workflow.Core.Activities.Abstracts;

public abstract class CompositeActivity : Activity, IVariableContainer, IComposite
{
    /// <inheritdoc />
    protected CompositeActivity() : base()
    {
        OnSignalReceived<CompleteCompositeSignal>(OnCompleteCompositeSignal);
    }

    /// <inheritdoc />
    [JsonIgnore]  // Composite activities' Variables is intended to be constructed from code only.
    public ICollection<Variable> Variables { get; init; } = new List<Variable>();

    [JsonIgnore]
    public Variable? ResultVariable { get; set; }

    [Port]
    [Browsable(false)]
    public IActivity Root { get; set; } = new Sequence();

    public virtual void Setup() { }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ConfigureActivities(context);

        // Register variables.
        foreach (var variable in Variables)
            variable.Set(context, variable.Value);

        await context.ScheduleActivityAsync(Root, OnRootCompletedAsync);
    }

    /// <summary>
    /// Override this method to configure activity properties before execution.
    /// </summary>
    protected virtual void ConfigureActivities(ActivityExecutionContext context)
    {
    }

    private async ValueTask OnRootCompletedAsync(ActivityCompletedContext context)
    {
        await OnCompletedAsync(context);
        await context.TargetContext.CompleteActivityAsync();
    }

    /// <summary>
    /// Completes this composite activity.
    /// </summary>
    protected async Task CompleteAsync(ActivityExecutionContext context, object? result = null) => await context.SendSignalAsync(new CompleteCompositeSignal(result));

    /// <summary>
    /// Completes this composite activity.
    /// </summary>
    protected async Task CompleteAsync(ActivityExecutionContext context, params string[] outcomes) => await CompleteAsync(context, new Outcomes(outcomes));

    /// <summary>
    /// Override this method to execute custom logic when the composite activity completes.
    /// </summary>
    /// <param name="context">The context of the composite activity.</param>
    protected virtual ValueTask OnCompletedAsync(ActivityCompletedContext context)
    {
        return new();
    }

    private async ValueTask OnCompleteCompositeSignal(CompleteCompositeSignal signal, SignalContext context)
    {
        // Set the outcome into the context for the parent activity to pick up.
        context.SenderActivityExecutionContext.WorkflowExecutionContext.TransientProperties[nameof(CompleteCompositeSignal)] = signal;

        var completedContext = new ActivityCompletedContext(context.ReceiverActivityExecutionContext, context.SenderActivityExecutionContext, signal.Value);
        await OnCompletedAsync(completedContext);

        // Complete the sender first so that it notifies its parents to complete.
        await context.SenderActivityExecutionContext.CompleteActivityAsync();

        // Then complete this activity.
        await context.ReceiverActivityExecutionContext.CompleteActivityAsync(signal.Value);
        context.StopPropagation();

    }
}

public abstract class CompositeActivity<T> : CompositeActivity, IActivityWithResult<T>
{
    /// <inheritdoc />
    protected CompositeActivity() : base()
    {
    }

    /// <summary>
    /// The result of the activity.
    /// </summary>
    [Output] public Output<T>? Result { get; set; }

    Output? IActivityWithResult.Result
    {
        get => Result;
        set => Result = (Output<T>?)value;
    }
}
