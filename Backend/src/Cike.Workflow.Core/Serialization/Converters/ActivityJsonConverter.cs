using Cike.Workflow.Common.Serialization;
using Cike.Workflow.Core.ActivityDescriptors;
using System.Reflection;

namespace Cike.Workflow.Core.Tests.Serializers
{
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

            return (IActivity)JsonSerializer.Deserialize(root, activityDescriptor?.ClrType ?? typeof(NotFoundActivity), JsonHelper.DefaultSerializerOptions)!;
        }

        public override void Write(Utf8JsonWriter writer, IActivity value, JsonSerializerOptions options)
        {
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

                var propName = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
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

                JsonSerializer.Serialize(writer, input, options);
            }

            writer.WriteEndObject();
        }
    }
}
