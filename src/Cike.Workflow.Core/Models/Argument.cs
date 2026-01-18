namespace Cike.Workflow.Core.Models;

public abstract class Argument
{
    protected Argument()
    {
    }

    /// <inheritdoc />
    protected Argument(MemoryBlockReference memoryBlockReference) : this(() => memoryBlockReference)
    {
    }

    protected Argument(Func<MemoryBlockReference> memoryBlockReference)
    {
        MemoryBlockReference = memoryBlockReference;
    }

    [JsonIgnore]
    public Func<MemoryBlockReference> MemoryBlockReference { get; set; } = null!;
}
