namespace Cike.Workflow.Core.Models;

/// <summary>
/// Base class for workflow input and output definitions.
/// </summary>
public abstract class ArgumentDefinition
{
    public string Type { get; set; } = "object";

    public bool IsArray { get; set; }

    public string Name { get; set; } = default!;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
