namespace Cike.Workflow.Core.ActivityDescriptors.Models;

public class OutputDescriptor : PropertyDescriptor
{
    public OutputDescriptor()
    {
    }

    /// <inheritdoc />
    public OutputDescriptor(
        string name,
        string clrName,
        string displayName,
        Type type,
        Func<IActivity, object?> valueGetter,
        Action<IActivity, object?> valueSetter,
        bool? isSerializable = default)
    {
        Name = name;
        ClrName = clrName;
        DisplayName = displayName;
        Type = type;
        ValueGetter = valueGetter;
        ValueSetter = valueSetter;
        IsSerializable = isSerializable;
    }
}
