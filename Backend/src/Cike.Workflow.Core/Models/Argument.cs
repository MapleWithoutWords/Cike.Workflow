using System.Text.Json.Serialization;

namespace Cike.Workflow.Core.Models;

/// <summary>
/// A base type for the <see cref="Input{T}"/> type.
/// </summary>
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
