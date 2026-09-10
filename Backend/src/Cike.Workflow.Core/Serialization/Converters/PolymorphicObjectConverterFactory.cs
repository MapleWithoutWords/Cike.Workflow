using Cike.Workflow.Common.Serialization;
using Cike.Workflow.Common.Serialization.Internals;
using System.Dynamic;

namespace Cike.Workflow.Core.Serialization.Converters;

/// <summary>
/// A JSON converter factory that creates <see cref="PolymorphicObjectConverter"/> instances.
/// </summary>
public class PolymorphicObjectConverterFactory : JsonConverterFactory
{
    private readonly ISerializationTypeRegistry _workflowJsonTypeRegistry;
    private readonly ILogger? _logger;

    /// <summary>
    /// A JSON converter factory that creates <see cref="PolymorphicObjectConverter"/> instances.
    /// </summary>
    public PolymorphicObjectConverterFactory(ISerializationTypeRegistry workflowJsonTypeRegistry, ILogger? logger = null)
    {
        _workflowJsonTypeRegistry = workflowJsonTypeRegistry;
        _logger = logger;
    }

    /// <summary>
    /// Default constructor for use with attributes.
    /// </summary>
    public PolymorphicObjectConverterFactory()
    {
        _workflowJsonTypeRegistry = SerializationTypeRegistry.CreateDefault();
    }

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsClass
               && typeToConvert == typeof(object)
               || typeToConvert == typeof(ExpandoObject)
               || typeToConvert == typeof(Dictionary<string, object>))
            return true;

        if (typeToConvert.IsInterface
               && typeToConvert == typeof(IDictionary<string, object>))
            return true;

        return false;
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeof(IDictionary<string, object>).IsAssignableFrom(typeToConvert))
            return new PolymorphicDictionaryConverter(options, _workflowJsonTypeRegistry);

        return new PolymorphicObjectConverter(_workflowJsonTypeRegistry, _logger);
    }
}
