namespace Cike.Workflow.Expressions.LiteralExpressions;

public class LiteralExpressionHandler : IExpressionHandler
{
    private readonly ISerializationTypeRegistry _serializationTypeRegistry;

    public LiteralExpressionHandler(ISerializationTypeRegistry serializationTypeRegistry)
    {
        _serializationTypeRegistry = serializationTypeRegistry;
    }

    public ValueTask<object?> EvaluateAsync(Expression expression, Type returnType, ExpressionExecutionContext context, ExpressionEvaluatorOptions options)
    {
        var value = expression.Value.ConvertTo(returnType, new ObjectConverterOptions(SerializationTypeRegistry: _serializationTypeRegistry));
        return ValueTask.FromResult(value);
    }
}
