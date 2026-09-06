using System.Runtime.CompilerServices;

namespace Cike.Workflow.Core.Activities;

public class ForEach<T> : Activity
{
    private const string CurrentIndexProperty = "CurrentIndex";

    /// <inheritdoc />
    public ForEach(ICollection<T> items) : this(new Input<ICollection<T>>(items))
    {
    }

    /// <inheritdoc />
    public ForEach(Input<ICollection<T>> items) : this()
    {
        Items = items;
    }

    /// <inheritdoc />
    public ForEach()
    {
        Behaviors.Add<BreakBehavior>(this);
    }

    [Input(Description = "The set of values to iterate.")]
    public Input<ICollection<T>> Items { get; set; } = new(Array.Empty<T>());

    [Port]
    public IActivity? Body { get; set; }

    [Output(Description = "Assign the current value to the specified variable.")]
    public Output<T>? CurrentValue { get; set; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        // Execute first iteration.
        await HandleIteration(context);
    }

    private async Task HandleIteration(ActivityExecutionContext context)
    {
        var isBreaking = context.GetIsBreakingProperty();

        if (isBreaking)
        {
            await context.CompleteActivityAsync();
            return;
        }

        var currentIndex = context.GetProperty<int>(CurrentIndexProperty);
        var items = context.Get(Items)!.ToList();

        if (currentIndex >= items.Count)
        {
            await context.CompleteActivityAsync();
            return;
        }

        var currentValue = items[currentIndex];
        context.Set(CurrentValue, currentValue);

        if (Body != null)
        {
            var variables = new[]
            {
                new Variable("CurrentIndex", currentIndex),
                new Variable("CurrentValue", currentValue)
            };
            await context.ScheduleActivityAsync(Body, OnChildCompleted, variables: variables);
        }
        else
            await context.CompleteActivityAsync();

        // Increment index.
        context.UpdateProperty<int>(CurrentIndexProperty, x => x + 1);
    }

    private async ValueTask OnChildCompleted(ActivityCompletedContext context)
    {
        await HandleIteration(context.TargetContext);
    }
}
