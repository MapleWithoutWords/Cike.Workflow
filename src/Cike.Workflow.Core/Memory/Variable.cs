using Cike.Workflow.Core.Contexts;
using Elsa.Expressions.Helpers;
using Elsa.Expressions.Models;
using Humanizer;

namespace Cike.Workflow.Core.Memory;

public class Variable : MemoryBlockReference
{
    public Variable()
    {
    }

    public Variable(string name)
    {
        Id = GetIdFromName(name);
        Name = name;
    }

    /// <inheritdoc />
    public Variable(string name, object? value = null) : this(name)
    {
        Value = value;
    }

    public Variable(string name, object? value = null, string? id = null) : this(name, value)
    {
        if (id != null)
        {
            Id = id;
        }
    }

    public string Name { get; set; } = null!;

    /// <summary>
    /// Ä¬ÈÏÖµ
    /// </summary>
    public object? Value { get; set; }

    /// <inheritdoc />
    public override MemoryBlock Declare() => new(Value, new VariableBlockMetadata(this,  false));

    private string GetIdFromName(string? name) => $"{name?.Camelize() ?? "Unnamed"}{nameof(Variable)}";
}

public class Variable<T> : Variable
{
    /// <inheritdoc />
    public Variable()
    {
    }

    /// <inheritdoc />
    public Variable(string name, T value) : base(name, value)
    {
    }

    public Variable(string name, T value, string? id = null) : base(name, value, id)
    {
    }

    /// <summary>
    /// Gets the value of the variable.
    /// </summary>
    public T? Get(ActivityExecutionContext context) => Get(context.ExpressionExecutionContext).ConvertTo<T?>();

    /// <summary>
    /// Gets the value of the variable.
    /// </summary>
    public new T? Get(ExpressionExecutionContext context) => base.Get(context).ConvertTo<T?>();

    public Variable<T> WithId(string id)
    {
        Id = id;
        return this;
    }

    public Variable<T> WithName(string name)
    {
        Name = name;
        return this;
    }

    public Variable<T> WithValue(T value)
    {
        Value = value;
        return this;
    }
}

/// <summary>
/// Provides metadata about the variable block.
/// </summary>
public record VariableBlockMetadata(Variable Variable, bool IsInitialized);