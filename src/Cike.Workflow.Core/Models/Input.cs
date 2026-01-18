namespace Cike.Workflow.Core.Models;

public abstract class Input : Argument
{
    /// <inheritdoc />
    protected Input(MemoryBlockReference memoryBlockReference, Type type) : base(memoryBlockReference)
    {
        Type = type;
    }

    /// <inheritdoc />
    protected Input(Expression? expression, MemoryBlockReference memoryBlockReference, Type type) : base(memoryBlockReference)
    {
        Expression = expression;
        Type = type;
    }

    public Expression? Expression { get; }

    [JsonPropertyName("typeName")]
    public Type Type { get; set; }
}

public class Input<T> : Input
{
    /// <inheritdoc />
    public Input(MemoryBlockReference memoryBlockReference) : base(memoryBlockReference, typeof(T))
    {
    }

    /// <inheritdoc />
    public Input(T literal, string? id = null) : this(new Literal<T>(literal, id))
    {
    }

    /// <inheritdoc />
    public Input(Func<T> @delegate, string? id = null) : this(Expression.DelegateExpression(@delegate), new(id!))
    {
    }

    /// <inheritdoc />
    public Input(Func<ExpressionExecutionContext, ValueTask<T?>> @delegate, string? id = null) : this(Expression.DelegateExpression(@delegate), new(id!))
    {
    }

    /// <inheritdoc />
    public Input(Func<ValueTask<T?>> @delegate, string? id = null) : this(Expression.DelegateExpression(@delegate), new(id!))
    {
    }

    /// <inheritdoc />
    public Input(Func<ExpressionExecutionContext, T> @delegate, string? id = null) : this(Expression.DelegateExpression(@delegate), new(id!))
    {
    }

    /// <inheritdoc />
    public Input(Variable variable) : base(new("Variable", variable), variable, typeof(T))
    {
    }

    /// <inheritdoc />
    public Input(Output output) : base(new("Output", output), output.MemoryBlockReference(), typeof(T))
    {
    }

    /// <inheritdoc />
    public Input(Literal<T> literal) : base(Expression.LiteralExpression(literal.Value), literal, typeof(T))
    {
    }

    /// <inheritdoc />
    public Input(Literal literal) : base(Expression.LiteralExpression(literal.Value), literal, typeof(T))
    {
    }

    /// <inheritdoc />
    public Input(ObjectLiteral<T> literal) : base(Expression.LiteralExpression(literal.Value), literal, typeof(T))
    {
    }

    /// <inheritdoc />
    public Input(ObjectLiteral literal) : base(Expression.LiteralExpression(literal.Value), literal, typeof(T))
    {
    }

    /// <inheritdoc />
    public Input(Expression expression, MemoryBlockReference memoryBlockReference) : base(expression, memoryBlockReference, typeof(T))
    {
    }

    /// <inheritdoc />
    public Input(Expression expression) : this(expression, new())
    {
    }
}
