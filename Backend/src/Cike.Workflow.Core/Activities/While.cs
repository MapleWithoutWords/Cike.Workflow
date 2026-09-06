using System.Runtime.CompilerServices;

namespace Cike.Workflow.Core.Activities;

/// <summary>
/// Execute an activity while a given condition evaluates to true.
/// </summary>
[Activity("Cike", "Looping", "Execute an activity while a given condition evaluates to true.")]
public class While : Activity
{
    /// <summary>
    /// Creates a <see cref="While"/> activity that loops forever.
    /// </summary>
    public static While True(IActivity body) => new(body)
    {
        Condition = new(true)
    };

    /// <inheritdoc />
    public While() : base()
    {
        Behaviors.Add<BreakBehavior>(this);
    }

    /// <inheritdoc />
    public While(IActivity? body = null) : this()
    {
        Body = body;
    }

    /// <inheritdoc />
    public While(Input<bool> condition, IActivity? body = null) : this(body)
    {
        Condition = condition;
    }

    /// <summary>
    /// The condition to evaluate.
    /// </summary>
    [Input(AutoEvaluate = false)]
    public Input<bool> Condition { get; set; } = new(false);

    /// <summary>
    /// The <see cref="IActivity"/> to execute on every iteration.
    /// </summary>
    [Port]
    public IActivity? Body { get; set; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context) => await HandleIterationAsync(context);

    private async ValueTask OnBodyCompleted(ActivityCompletedContext context)
    {
        await HandleIterationAsync(context.TargetContext, context.ChildContext);
    }

    private async ValueTask HandleIterationAsync(ActivityExecutionContext context, ActivityExecutionContext? completedChildContext = null)
    {
        var isBreaking = context.GetIsBreakingProperty();
        var loop = !isBreaking && await context.EvaluateInputPropertyAsync<While, bool>(x => x.Condition);

        if (loop)
        {
            var options = new ScheduleWorkOptions
            {
                CompletionCallback = OnBodyCompleted,
                SchedulingActivityExecutionId = completedChildContext?.Id
            };
            await context.ScheduleActivityAsync(Body, options);
        }
        else
            await context.CompleteActivityAsync();
    }
}
