namespace Cike.Workflow.Expression.Javascript.Activities;

[Activity("Cike", "Scripting", "Executes JavaScript code", DisplayName = "Run JavaScript")]
public class RunJavaScript : AutoCompleteActivity<object?>
{
    public RunJavaScript() : base()
    {
    }

    public RunJavaScript(string script) : this()
    {
        Script = new(script);
    }

    [Input]
    public Input<string> Script { get; set; } = new("");

    [Input(Description = "A list of possible outcomes.")]
    public Input<ICollection<string>> PossibleOutcomes { get; set; } = null!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var script = context.Get(Script);

        // If no script was specified, there's nothing to do.
        if (string.IsNullOrWhiteSpace(script))
            return;

        // Get a JavaScript evaluator.
        var javaScriptEvaluator = context.GetRequiredService<IJavaScriptEvaluator>();

        // Run the script.
        var result = await javaScriptEvaluator.EvaluateAsync(
            script,
            typeof(object),
            context.ExpressionExecutionContext,
            ExpressionEvaluatorOptions.Empty,
            engine => ConfigureEngine(engine, context),
            context.CancellationToken);

        // Set the result as output, if any.
        if (result is not null)
            context.Set(Result, result);

        // Get the outcome or outcomes set by the script, if any. If not set, use "Done".
        var outcomes = context.TransientProperties.GetValueOrDefault("Outcomes", () => new[] { "Done" })!;

        // Complete the activity with the outcome.
        await context.CompleteActivityAsync(new Outcomes(outcomes));
    }

    private static void ConfigureEngine(Engine engine, ActivityExecutionContext context)
    {
        engine.SetValue("setOutcome", (Action<string>)(value => context.TransientProperties["Outcomes"] = new[] { value }));
        engine.SetValue("setOutcomes", (Action<string[]>)(value => context.TransientProperties["Outcomes"] = value));
    }
}
