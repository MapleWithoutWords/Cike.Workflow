using Cike.Workflow.Core.Variables;

namespace Cike.Workflow.Core.Activities.Abstracts;

public abstract class ContainerActivity : Activity, IVariableContainer
{
    public ICollection<IActivity> Activities { get; set; } = new List<IActivity>();

    public ICollection<Variable> Variables { get; set; } = new Collection<Variable>();

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        // Ensure variables have names.
        EnsureNames(Variables);

        // Register variables.
        context.ExpressionExecutionContext.Memory.Declare(Variables);

        // Schedule children.
        await ScheduleChildrenAsync(context);
    }

    private void EnsureNames(IEnumerable<Variable> variables)
    {
        var count = 0;

        foreach (var variable in variables)
            if (string.IsNullOrWhiteSpace(variable.Name))
                variable.Name = $"Variable{++count}";
    }

    /// <summary>
    /// Schedule the <see cref="Activities"/> for execution.
    /// </summary>
    protected virtual ValueTask ScheduleChildrenAsync(ActivityExecutionContext context)
    {
        return ValueTask.CompletedTask;
    }
}
