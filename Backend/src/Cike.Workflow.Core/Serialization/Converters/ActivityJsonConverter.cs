using Cike.Workflow.Common.Serialization;
using Cike.Workflow.Core.ActivityDescriptors;
using System.Reflection;

namespace Cike.Workflow.Core.Serialization.Converters;

public class ActivityJsonConverter(IActivityRegistry activityRegistry) : JsonConverter<IActivity>
{
    public override IActivity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var activityTypeNameElement))
            throw new JsonException("Failed to extract activity type property");
        var activityTypeName = activityTypeNameElement.GetString()!;

        var activityDescriptor = activityRegistry.Find(activityTypeName);

        return (IActivity)JsonSerializer.Deserialize(root, activityDescriptor?.ClrType ?? typeof(NotFoundActivity), GetClonedOptions(JsonHelper.DefaultSerializerOptions))!;
    }

    public override void Write(Utf8JsonWriter writer, IActivity value, JsonSerializerOptions options)
    {
        var clonedOptions = GetClonedWriterOptions(options);
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }
        var activityDescriptor = activityRegistry.Find(value.Type);
        if (activityDescriptor == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                continue;

            var propName = clonedOptions.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
            writer.WritePropertyName(propName);
            var input = property.GetValue(value);

            if (input == null)
            {
                writer.WriteNullValue();
                continue;
            }

            if (property.Name == nameof(IActivity.CustomProperties))
            {
                var customProperties = new Dictionary<string, object>(value.CustomProperties);
                foreach (var kvp in customProperties)
                {
                    if (kvp.Value is IActivity or IEnumerable<IActivity>)
                        customProperties.Remove(kvp.Key);
                }

                input = customProperties;
            }

            JsonSerializer.Serialize(writer, input, clonedOptions);
        }

        writer.WriteEndObject();
    }

    private JsonSerializerOptions GetClonedOptions(JsonSerializerOptions options)
    {
        var clonedOptions = new JsonSerializerOptions(options);
        //clonedOptions.Converters.Add(new PolymorphicObjectConverterFactory());

        // 本转换器必须随克隆传递：Read 内部按具体 CLR 类型反序列化时，
        // 其 IActivity 类型成员（如 Flowchart.Activities / Start）需要再次路由回本转换器，
        // 否则嵌套活动节点反序列化失败（接口类型不支持）。
        if (clonedOptions.Converters.All(x => x is not ActivityJsonConverter))
            clonedOptions.Converters.Add(this);

        return clonedOptions;
    }

    private JsonSerializerOptions GetClonedWriterOptions(JsonSerializerOptions options)
    {
        var clonedOptions = GetClonedOptions(options);
        //clonedOptions.Converters.Add(new JsonIgnoreCompositeRootConverterFactory(serviceProvider.GetRequiredService<ActivityWriter>()));
        return clonedOptions;
    }
}
