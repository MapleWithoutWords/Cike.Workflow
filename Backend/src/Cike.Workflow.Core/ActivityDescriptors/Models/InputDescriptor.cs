namespace Cike.Workflow.Core.ActivityDescriptors.Models;

public class InputDescriptor : PropertyDescriptor
{
    public InputDescriptor()
    {
    }

    public InputDescriptor(
        string name,
        string clrName,
        Type type,
        Func<IActivity, object?> valueGetter,
        Action<IActivity, object?> valueSetter,
        string uiComponentName,
        bool isWrapped,
        string displayName,
        string? description = null,
        object? defaultValue = null,
        bool isBrowsable = true,
        bool isSerializable = true,
        bool autoEvaluate = true)
    {
        Name = name;
        ClrName = clrName;
        Type = type;
        ValueGetter = valueGetter;
        ValueSetter = valueSetter;
        UIComponentName = uiComponentName;
        IsWrapped = isWrapped;
        DisplayName = displayName;
        Description = description;
        DefaultValue = defaultValue;
        IsBrowsable = isBrowsable;
        IsSerializable = isSerializable;
        AutoEvaluate = autoEvaluate;
    }

    public string UIComponentName { get; set; } = null!;

    public object? DefaultValue { get; set; }

    public bool? IsReadOnly { get; set; }

    public bool IsWrapped { get; set; }

    public bool AutoEvaluate { get; set; } = true;
}
