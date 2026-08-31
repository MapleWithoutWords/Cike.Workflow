namespace Cike.Workflow.Core.ActivityDescriptors.Models;

public class InputDescriptor : PropertyDescriptor
{
    public InputDescriptor()
    {
    }

    public InputDescriptor(
        string name,
        Type type,
        Func<IActivity, object?> valueGetter,
        Action<IActivity, object?> valueSetter,
        bool isWrapped,
        string displayName,
        bool isSerializable = true,
        bool autoEvaluate = true,
        string? evaluatorType = null)
    {
        Name = name;
        Type = type;
        ValueGetter = valueGetter;
        ValueSetter = valueSetter;
        IsWrapped = isWrapped;
        DisplayName = displayName;
        AutoEvaluate = autoEvaluate;
        EvaluatorType = evaluatorType;
        IsSerializable = isSerializable;
    }

    public bool IsWrapped { get; set; }

    public bool AutoEvaluate { get; set; } = true;

    public string? EvaluatorType { get; set; }
}
