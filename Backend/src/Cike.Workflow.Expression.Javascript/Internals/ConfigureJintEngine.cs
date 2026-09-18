using Cike.Core.DependencyInjection;
using Cike.Workflow.Common.Serialization;
using Cike.Workflow.Core.Extensions;
using Cike.Workflow.Expression.Javascript.Extensions;
using Cike.Workflow.Expression.Javascript.Options;
using Jint.Runtime.Descriptors;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace Cike.Workflow.Expression.Javascript.Internals;

public class ConfigureJintEngine(ISerializationTypeRegistry serializationTypeRegistry, IOptions<JintOptions> jintOptions) : IScopedDependency
{
    private static readonly Type[] BlacklistedTypes =
    [
        typeof(string),
        typeof(object),
        // Add more types if needed.
    ];
    private readonly JsonSerializerOptions _jsonSerializerOptions = CreateJsonSerializerOptions();

    public Jint.Options ConfigureTypes(Jint.Options options, CancellationToken cancellationToken)
    {
        options.RegisterType<DateTime>();
        options.RegisterType<DateTimeOffset>();
        options.RegisterType<TimeSpan>();
        options.RegisterType<Guid>();
        options.RegisterType<Random>();

        foreach (var type in serializationTypeRegistry
                .ListTypes()
                .Where(x => x is { ContainsGenericParameters: false } && !BlacklistedTypes.Contains(x) && !x.IsPrimitive))
            options.RegisterType(type);

        return options;
    }

    public Engine ConfigureEngineFunctions(Engine engine, ExpressionExecutionContext context)
    {
        AddFunction("setVariable", e => JsValue.FromObject(e, (Action<string, object>)((name, value) =>
        {
            context.SetVariableInScope(name, value);
        })));
        AddFunction("getVariable", e => JsValue.FromObject(e, (Func<string, object?>)(name => context.GetVariableInScope(name))));
        AddFunction("getInput", e => JsValue.FromObject(e, (Func<string, object?>)(name => context.GetInput(name))));
        AddFunction("getOutputFrom", e => JsValue.FromObject(e, (Func<string, string?, object?>)((activityIdName, outputName) => context.GetOutput(activityIdName, outputName))));
        AddFunction("isNullOrWhiteSpace", e => JsValue.FromObject(e, (Func<string, bool>)(value => string.IsNullOrWhiteSpace(value))));
        AddFunction("isNullOrEmpty", e => JsValue.FromObject(e, (Func<string, bool>)(value => string.IsNullOrEmpty(value))));
        AddFunction("toJson", e => JsValue.FromObject(e, (Func<object, string>)(value => Serialize(value))));
        AddFunction("parseGuid", e => JsValue.FromObject(e, (Func<string, Guid>)(value => Guid.Parse(value))));
        AddFunction("newGuid", e => JsValue.FromObject(e, (Func<Guid>)(() => Guid.NewGuid())));
        AddFunction("newGuidString", e => JsValue.FromObject(e, (Func<string>)(() => Guid.NewGuid().ToString())));
        AddFunction("newShortGuid", e => JsValue.FromObject(e, (Func<string>)(() => Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", ""))));
        AddFunction("bytesToString", e => JsValue.FromObject(e, (Func<byte[], string>)(value => Encoding.UTF8.GetString(value))));
        AddFunction("bytesFromString", e => JsValue.FromObject(e, (Func<string, byte[]>)(value => Encoding.UTF8.GetBytes(value))));
        AddFunction("bytesToBase64", e => JsValue.FromObject(e, (Func<byte[], string>)(value => Convert.ToBase64String(value))));
        AddFunction("bytesFromBase64", e => JsValue.FromObject(e, (Func<string, byte[]>)(value => Convert.FromBase64String(value))));
        AddFunction("stringToBase64", e => JsValue.FromObject(e, (Func<string, string>)(value => Convert.ToBase64String(Encoding.UTF8.GetBytes(value)))));
        AddFunction("stringFromBase64", e => JsValue.FromObject(e, (Func<string, string>)(value => Encoding.UTF8.GetString(Convert.FromBase64String(value)))));
        AddFunction("streamToBytes", e => JsValue.FromObject(e, (Func<Stream, byte[]>)(value => StreamToBytes(value))));
        AddFunction("streamToBase64", e => JsValue.FromObject(e, (Func<Stream, string>)(value => Convert.ToBase64String(StreamToBytes(value)))));
        AddFunction("getGuidString", e => JsValue.FromObject(e, (Func<string>)(() => Guid.NewGuid().ToString())));
        AddFunction("getShortGuid", e => JsValue.FromObject(e, (Func<string>)(() => Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", ""))));

        return engine;

        void AddFunction(string name, Func<Engine, JsValue> valueFactory) => engine.Advanced.AddLazyGlobal(name, valueFactory, PropertyFlag.NonEnumerable);
    }

    private string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, _jsonSerializerOptions);
    }

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private byte[] StreamToBytes(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
