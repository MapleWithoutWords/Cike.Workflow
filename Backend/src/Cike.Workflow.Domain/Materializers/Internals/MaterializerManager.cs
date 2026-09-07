using Cike.Core.DependencyInjection;

namespace Cike.Workflow.Domain.Materializers.Internals;

/// <inheritdoc />
public class MaterializerManager(Func<IEnumerable<IWorkflowMaterializer>> materializers) : IMaterializerManager, IScopedDependency
{
    private readonly Lazy<IReadOnlyCollection<IWorkflowMaterializer>> _materializers = new(() => materializers().ToArray());

    /// <inheritdoc />
    public IWorkflowMaterializer? GetMaterializer(string name)
    {
        return _materializers.Value.FirstOrDefault(x => x.Name == name);
    }

    /// <inheritdoc />
    public bool IsMaterializerAvailable(string name)
    {
        return _materializers.Value.Any(x => x.Name == name);
    }
}
