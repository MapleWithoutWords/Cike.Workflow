using System.Runtime.CompilerServices;

namespace Cike.Workflow.Core.Activities;

[FlowNode("True", "False")]
[Activity("Cike", "Branching", "Evaluate a Boolean condition to determine which path to execute next.", DisplayName = "Decision")]
public class If : Activity
{
    /// <inheritdoc />
    public If() : base()
    {
    }

    /// <summary>
    /// The condition to evaluate.
    /// </summary>
    [Input]
    public Input<bool> Condition { get; set; } = new(new Literal<bool>(false));

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var result = context.Get(Condition);
        var outcome = result ? "True" : "False";

        await context.CompleteActivityAsync(new Outcomes(outcome));
    }
}
