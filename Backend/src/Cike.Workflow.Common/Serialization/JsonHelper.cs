using Cike.Workflow.Common.Serialization.Converters;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;

namespace Cike.Workflow.Common.Serialization;

public static class JsonHelper
{
    static JsonHelper()
    {
        DefaultSerializerOptions = CreateOptionsInternal();
    }

    public static JsonSerializerOptions DefaultSerializerOptions;

    public static JsonSerializerOptions CreateOptionsInternal()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(JsonMetadataServices.TimeSpanConverter);
        options.Converters.Add(new IntegerJsonConverter());
        options.Converters.Add(new BigIntegerJsonConverter());
        options.Converters.Add(new DecimalJsonConverter());
        options.Converters.Add(new ExpandoObjectConverterFactory());

        return options;
    }
}
