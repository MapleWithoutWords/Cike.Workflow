namespace Cike.Workflow.Core.Models;

/// <summary>
/// A definition of a workflow's input.
/// </summary>
public class InputDefinition : ArgumentDefinition
{
    /// <summary>
    /// The type of the storage driver to use for this input.
    /// </summary>
    public string? StorageDriverType { get; set; }
}
