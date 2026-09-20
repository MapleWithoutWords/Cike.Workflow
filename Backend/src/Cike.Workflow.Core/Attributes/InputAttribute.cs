namespace Cike.Workflow.Core.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class InputAttribute : Attribute
{
    public string? Name { get; set; }

    public string? DisplayName { get; set; }

    public string? UIComponentName { get; set; }

    public string? Description { get; set; }

    public string? Category { get; set; }

    public object? Options { get; set; }

    public float Order { get; set; }

    public object? DefaultValue { get; set; }

    public bool IsReadOnly { get; set; }

    public bool IsBrowsable { get; set; } = true;

    public bool AutoEvaluate { get; set; } = true;

    public bool IsSerializable { get; set; } = true;

    public bool CanContainSecrets { get; set; }
}
