using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cike.Workflow.Core.Activities;

[FlowNode("Default")]
[Activity("Cike")]
public class Switch : Activity
{
    public Switch() : base()
    {
    }

    [Input()]
    public ICollection<FlowSwitchCase> Cases { get; set; } = new List<FlowSwitchCase>();

    [Input(
        Description = "The switch mode determines whether the first match should be scheduled, or all matches."
    )]
    public Input<SwitchMode> Mode { get; set; } = new(SwitchMode.MatchFirst);

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var matchingCases = (await FindMatchingCasesAsync(context.ExpressionExecutionContext)).ToList();
        var hasAnyMatches = matchingCases.Any();
        var mode = context.Get(Mode);
        var results = mode == SwitchMode.MatchFirst ? hasAnyMatches ? [matchingCases.First()] : Array.Empty<FlowSwitchCase>() : matchingCases.ToArray();
        var outcomes = hasAnyMatches ? results.Select(r => r.Label).ToArray() : ["Default"];

        await context.CompleteActivityAsync(new Outcomes(outcomes));
    }

    private async Task<IEnumerable<FlowSwitchCase>> FindMatchingCasesAsync(ExpressionExecutionContext context)
    {
        var matchingCases = new List<FlowSwitchCase>();
        var expressionEvaluator = context.GetRequiredService<IExpressionEvaluator>();

        foreach (var switchCase in Cases)
        {
            var result = await expressionEvaluator.EvaluateAsync<bool?>(switchCase.Condition, context);

            if (result == true)
            {
                matchingCases.Add(switchCase);
            }
        }

        return matchingCases;
    }
}

public class FlowSwitchCase
{
    [JsonConstructor]
    public FlowSwitchCase()
    {
    }

    public FlowSwitchCase(string label, Expression condition)
    {
        Label = label;
        Condition = condition;
    }

    public string Label { get; set; } = null!;

    public Expression Condition { get; set; } = Expression.LiteralExpression(false);
}

public enum SwitchMode
{
    /// <summary>
    /// Yields the outcome of the first condition evaluating to true.
    /// </summary>
    MatchFirst,

    /// <summary>
    /// Yields the outcome of all conditions evaluating to true.
    /// </summary>
    MatchAny
}
