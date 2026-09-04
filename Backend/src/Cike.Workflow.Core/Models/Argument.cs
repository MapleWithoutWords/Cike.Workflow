using System.Text.Json.Serialization;

namespace Cike.Workflow.Core.Models;

public abstract class Argument
{
    public Argument()
    {
        MemoryBlockReference = new MemoryBlockReference();
    }

    public Argument(MemoryBlockReference memoryBlockReference) : this()
    {
        MemoryBlockReference = memoryBlockReference;
    }

    public MemoryBlockReference MemoryBlockReference { get; set; } = null!;
}
