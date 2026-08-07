namespace Cike.Workflow.Core.Models;

/// <summary>
/// Base class for workflow input and output definitions.
/// </summary>
public abstract class ArgumentDefinition
{
    public Type Type { get; set; } = typeof(object);

    public string Name { get; set; } = default!;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
