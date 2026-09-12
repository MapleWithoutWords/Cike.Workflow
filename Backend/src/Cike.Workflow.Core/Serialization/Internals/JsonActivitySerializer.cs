using Cike.Workflow.Common.Serialization;
using Cike.Workflow.Core.Serialization.Converters;

namespace Cike.Workflow.Core.Serialization.Internals;

public class JsonActivitySerializer(IServiceProvider serviceProvider) : IActivitySerializer, ISingletonDependency
{
    /// <inheritdoc />
    public string Serialize(IActivity activity)
    {
        var options = GetOptionsInternal();
        return JsonSerializer.Serialize(activity, activity.GetType(), options);
    }

    /// <inheritdoc />
    public string Serialize(object value)
    {
        var options = GetOptionsInternal();
        return JsonSerializer.Serialize(value, options);
    }

    /// <inheritdoc />
    public IActivity Deserialize(string serializedActivity) => JsonSerializer.Deserialize<IActivity>(serializedActivity, GetOptions())!;

    /// <inheritdoc />
    public object Deserialize(string serializedValue, Type type) => JsonSerializer.Deserialize(serializedValue, type, GetOptions())!;

    /// <inheritdoc />
    public T Deserialize<T>(string serializedValue) => JsonSerializer.Deserialize<T>(serializedValue, GetOptions())!;

    /// <inheritdoc />
    private JsonSerializerOptions GetOptionsInternal()
    {
        var options = JsonHelper.CreateOptionsInternal();

        options.Converters.Add(ActivatorUtilities.CreateInstance<ActivityJsonConverter>(serviceProvider));
        //options.Converters.Add(ActivatorUtilities.CreateInstance<InputJsonConverterFactory>(serviceProvider));

        return options;
    }

    private JsonSerializerOptions? _options;
    /// <inheritdoc />
    public JsonSerializerOptions GetOptions()
    {
        if (_options != null)
            return _options;

        var options = JsonHelper.CreateOptionsInternal();
        options.Converters.Add(ActivatorUtilities.CreateInstance<ActivityJsonConverter>(serviceProvider));
        //options.Converters.Add(ActivatorUtilities.CreateInstance<InputJsonConverterFactory>(serviceProvider));
        _options = options;
        return _options;
    }
}
