namespace Cike.Workflow.Core.Serialization.Internals;

/// <summary>
/// Serializes and deserializes workflow states from and to JSON.
/// </summary>
public class JsonWorkflowStateSerializer : IWorkflowStateSerializer, ISingletonDependency
{
    private readonly ISerializationTypeRegistry _workflowJsonTypeRegistry;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonWorkflowStateSerializer"/> class.
    /// </summary>
    public JsonWorkflowStateSerializer(ISerializationTypeRegistry workflowJsonTypeRegistry, ILoggerFactory loggerFactory)
    {
        _workflowJsonTypeRegistry = workflowJsonTypeRegistry;
        _loggerFactory = loggerFactory;
    }

    public string Serialize(WorkflowState workflowState)
    {
        var options = GetOptions();
        return JsonSerializer.Serialize(workflowState, options);
    }

    public byte[] SerializeToUtfBytes(WorkflowState workflowState)
    {
        var options = GetOptions();
        return JsonSerializer.SerializeToUtf8Bytes(workflowState, options);
    }

    public JsonElement SerializeToElement(WorkflowState workflowState)
    {
        var options = GetOptions();
        return JsonSerializer.SerializeToElement(workflowState, options);
    }

    public string Serialize(object workflowState)
    {
        var options = GetOptions();
        return JsonSerializer.Serialize(workflowState, workflowState.GetType(), options);
    }

    public WorkflowState Deserialize(string serializedState)
    {
        var options = GetOptions();
        return JsonSerializer.Deserialize<WorkflowState>(serializedState, options)!;
    }

    public WorkflowState Deserialize(JsonElement serializedState)
    {
        var options = GetOptions();
        return serializedState.Deserialize<WorkflowState>(options)!;
    }

    public T Deserialize<T>(string serializedState)
    {
        var options = GetOptions();
        return JsonSerializer.Deserialize<T>(serializedState, options)!;
    }

    private JsonSerializerOptions? _options;
    /// <inheritdoc />
    public JsonSerializerOptions GetOptions()
    {
        if (_options != null)
            return _options;

        var options = JsonHelper.CreateOptionsInternal();
        options.Converters.Add(new TypeJsonConverter(_workflowJsonTypeRegistry));
        options.Converters.Add(new PolymorphicObjectConverterFactory(_workflowJsonTypeRegistry, _loggerFactory.CreateLogger<PolymorphicObjectConverter>()));
        _options = options;
        return new(options) { ReferenceHandler = new CrossScopedReferenceHandler() };
    }
}
