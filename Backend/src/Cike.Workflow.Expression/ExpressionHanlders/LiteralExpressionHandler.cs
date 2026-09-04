using Cike.Core.DependencyInjection;
using Cike.Workflow.Common.Serialization;
using Cike.Workflow.Expressions.Extensions;

namespace Cike.Workflow.Expressions.ExpressionHanlders;

public class LiteralExpressionHandler : IExpressionHandler, IScopedDependency
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
