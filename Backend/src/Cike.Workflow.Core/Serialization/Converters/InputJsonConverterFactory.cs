namespace Cike.Workflow.Core.Serialization.Converters;

/// <summary>
/// A JSON converter factory that creates <see cref="InputJsonConverter{T}"/> instances.
/// </summary>
public class InputJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeof(Input).IsAssignableFrom(typeToConvert) && typeToConvert.IsGenericType;

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var type = typeToConvert.GetGenericArguments().First();

        return (JsonConverter)Activator.CreateInstance(typeof(InputJsonConverter<>).MakeGenericType(type))!;
    }
}
