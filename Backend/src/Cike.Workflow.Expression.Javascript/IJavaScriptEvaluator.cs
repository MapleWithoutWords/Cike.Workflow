namespace Cike.Workflow.Expression.Javascript;

public interface IJavaScriptEvaluator
{
    Task<object?> EvaluateAsync(
        string expression,
        Type returnType,
        ExpressionExecutionContext context,
        ExpressionEvaluatorOptions? options = default,
        Action<Engine>? configureEngine = default,
        CancellationToken cancellationToken = default);
}
