using Cike.Workflow.Expressions.Extensions;
using Cike.Workflow.Expressions.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Expressions.Contexts;

public class ExpressionExecutionContext(
    IServiceProvider serviceProvider,
    MemoryRegister memory,
    ExpressionExecutionContext? parentContext = null,
    IDictionary<object, object>? transientProperties = null,
    Action? onChange = null,
    CancellationToken cancellationToken = default)
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public MemoryRegister Memory { get; } = memory;

    public IDictionary<object, object> TransientProperties { get; set; } = transientProperties ?? new Dictionary<object, object>();

    public ExpressionExecutionContext? ParentContext { get; set; } = parentContext;

    public CancellationToken CancellationToken { get; } = cancellationToken;

    public MemoryBlock GetBlock(Func<MemoryBlockReference> blockReference) => GetBlock(blockReference());

    public MemoryBlock GetBlock(MemoryBlockReference blockReference) => GetBlockInternal(blockReference) ?? throw new Exception($"Failed to retrieve memory block with reference {blockReference.Id}");

    public bool TryGetBlock(MemoryBlockReference blockReference, out MemoryBlock block)
    {
        var b = GetBlockInternal(blockReference);
        block = b ?? null!;
        return b != null;
    }

    public object? Get(Func<MemoryBlockReference> blockReference) => Get(blockReference());

    public object? Get(MemoryBlockReference blockReference) => GetBlock(blockReference).Value;

    public bool TryGet(MemoryBlockReference blockReference, out object? value)
    {
        if (TryGetBlock(blockReference, out var block))
        {
            value = block.Value;
            return true;
        }

        value = null;
        return false;
    }

    public T? Get<T>(Func<MemoryBlockReference> blockReference) => Get<T>(blockReference());

    public T? Get<T>(MemoryBlockReference blockReference) => Get(blockReference).ConvertTo<T>();

    public void Set(Func<MemoryBlockReference> blockReference, object? value, Action<MemoryBlock>? configure = null) => Set(blockReference(), value, configure);

    public void Set(MemoryBlockReference blockReference, object? value, Action<MemoryBlock>? configure = null)
    {
        var block = GetBlockInternal(blockReference) ?? Memory.Declare(blockReference);
        block.Value = value;
        configure?.Invoke(block);
        onChange?.Invoke();
    }

    public T GetRequiredService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

    private MemoryBlock? GetBlockInternal(MemoryBlockReference blockReference)
    {
        if (blockReference.Id == null!)
            return null;

        var currentContext = this;

        while (currentContext != null)
        {
            var register = currentContext.Memory;

            if (register.TryGetBlock(blockReference.Id, out var block))
                return block;

            currentContext = currentContext.ParentContext;
        }

        return null;
    }
}
