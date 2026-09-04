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
        bool isWrapped,
        string displayName,
        bool isSerializable = true,
        bool autoEvaluate = true)
    {
        Name = name;
        ClrName = clrName;
        Type = type;
        ValueGetter = valueGetter;
        ValueSetter = valueSetter;
        IsWrapped = isWrapped;
        DisplayName = displayName;
        AutoEvaluate = autoEvaluate;
        IsSerializable = isSerializable;
    }

    public bool IsWrapped { get; set; }

    public bool AutoEvaluate { get; set; } = true;
}
