namespace Cike.Workflow.Expression.Javascript.Expressions;

public class JavaScriptExpressionHandler(IJavaScriptEvaluator javaScriptEvaluator) : IExpressionHandler
{
    public async ValueTask<object?> EvaluateAsync(Workflow.Expressions.Models.Expression expression, Type returnType, ExpressionExecutionContext context, ExpressionEvaluatorOptions options)
    {
        var javaScriptExpression = expression.Value.ConvertTo<string>() ?? "";
        return await javaScriptEvaluator.EvaluateAsync(javaScriptExpression, returnType, context, options, engine => ConfigureEngine(engine, options), context.CancellationToken);
    }

    private void ConfigureEngine(Engine engine, ExpressionEvaluatorOptions options)
    {
        var args = new ExpandoObject() as IDictionary<string, object>;

        foreach (var (name, value) in options.Arguments)
            args[name.Camelize()] = value;

        engine.SetValue("args", args);
    }
}
