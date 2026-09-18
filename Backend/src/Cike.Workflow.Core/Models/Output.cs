namespace Cike.Workflow.Core.Models;

public class Output : Argument
{
    public Output() : base(new MemoryBlockReference())
    {
    }

    public Output(MemoryBlockReference memoryBlockReference) : base(memoryBlockReference)
    {
    }

    public object? ParseValue(object? value)
    {
        var genericType = GetType();
        return Variable.ParseValue(genericType, value);
    }

    public Type? GetTargetType(ActivityExecutionContext context)
    {
        var memoryBlockReference = this.MemoryBlockReference;

        if (memoryBlockReference is null)
            return null;

        if (!context.ExpressionExecutionContext.TryGetBlock(memoryBlockReference, out var memoryBlock))
            return null;

        var parsedContentVariableType = (memoryBlock.Metadata as VariableBlockMetadata)?.Variable.GetType();
        return parsedContentVariableType?.GenericTypeArguments.FirstOrDefault();
    }
}

public class Output<T> : Output
{
    public Output()
    {
    }

    public Output(MemoryBlockReference memoryBlockReference) : base(memoryBlockReference)
    {
    }
}
