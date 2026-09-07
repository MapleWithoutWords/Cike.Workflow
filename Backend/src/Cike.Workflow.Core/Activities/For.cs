using System.Runtime.CompilerServices;

namespace Cike.Workflow.Core.Activities;

[Activity("Cike", "Looping", "Iterate over a sequence of steps between a start and an end number.")]
public class For : Activity
{
    private const string CurrentStepProperty = "CurrentStep";

    /// <inheritdoc />
    public For() : base()
    {
        Behaviors.Add<BreakBehavior>(this);
    }

    /// <inheritdoc />
    public For(int start, int end, int step) : this()
    {
        Start = new(start);
        End = new(end);
        Step = new(step);
    }

    [Input(Description = "The start step.")]
    public Input<int> Start { get; set; } = new(0);

    [Input(Description = "The end step.")]
    public Input<int> End { get; set; } = new(0);

    [Input(Description = "The step size. To count down, enter a negative number.")]
    public Input<int> Step { get; set; } = new(1);

    [Input(Description = "Controls whether the end step is upper/lowerbound inclusive or exclusive. True (inclusive) by default.", DefaultValue = true)]
    public Input<bool> OuterBoundInclusive { get; set; } = new(true);

    [Port]
    public IActivity? Body { get; set; }

    [Output]
    public Output<object?>? CurrentValue { get; set; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var iterateNode = Body;

        if (iterateNode == null)
        {
            await context.CompleteActivityAsync();
            return;
        }

        await HandleIteration(context);
    }

    private async ValueTask HandleIteration(ActivityExecutionContext context)
    {
        var iterateNode = Body;
        var end = context.Get(End);
        var currentValue = context.GetProperty<int?>(CurrentStepProperty);
        var start = context.Get(Start);
        var step = context.Get(Step);
        var inclusive = context.Get(OuterBoundInclusive);
        var increment = step >= 0;

        currentValue = currentValue == null ? start : currentValue + step;

        var isBreaking = context.GetIsBreakingProperty();

        var loop =
            !isBreaking && (increment && inclusive ? currentValue <= end
                : increment && !inclusive ? currentValue < end
                : !increment && inclusive ? currentValue >= end
                : !increment && !inclusive && currentValue > end);

        if (loop)
        {
            if (iterateNode != null)
            {
                var variables = new[]
                {
                    new Variable("CurrentValue", currentValue)
                };
                await context.ScheduleActivityAsync(iterateNode, OnChildComplete, variables: variables);
            }


            // Update internal step.
            context.SetProperty(CurrentStepProperty, currentValue);

            // Update loop variable.
            context.Set(CurrentValue, currentValue);
        }
        else
        {
            // Report activity completion.
            await context.CompleteActivityAsync();
        }
    }

    private async ValueTask OnChildComplete(ActivityCompletedContext context) => await HandleIteration(context.TargetContext);
}
