namespace Cike.Workflow.Core.Models;

public class Output : Argument
{
    public Output() : base(new MemoryBlockReference())
    {
    }

    public Output(MemoryBlockReference memoryBlockReference) : base(memoryBlockReference)
    {
    }

    public Output(Func<MemoryBlockReference> memoryBlockReference) : base(memoryBlockReference)
    {
    }

    public object? ParseValue(object? value)
    {
        var genericType = GetType();
        return Variable.ParseValue(genericType, value);
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

    public Output(Func<MemoryBlockReference> memoryBlockReference) : base(memoryBlockReference)
    {
    }
}
