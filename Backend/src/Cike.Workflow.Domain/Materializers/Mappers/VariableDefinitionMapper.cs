namespace Cike.Workflow.Domain.Materializers.Mappers;

public class VariableDefinitionMapper(ISerializationTypeRegistry workflowJsonTypeRegistry, IServiceScopeFactory scopeFactory, ILogger<VariableDefinitionMapper> logger) : ISingletonDependency
{
    public Variable? Map(VariableDefinition source)
    {
        var type = SerializationTypeResolver.TryResolveType(workflowJsonTypeRegistry, source.TypeName, out var resolvedType) ? resolvedType : null;

        if (type == null)
        {
            logger.LogWarning("Failed to resolve the type {TypeName} of variable {VariableName}. Variable will not be mapped.", source.TypeName, source.Name);
            return null;
        }

        var valueType = type.IsArray ? source.IsArray ? type.MakeArrayType() : type : source.IsArray ? type.MakeArrayType() : type;
        var variableGenericType = typeof(Variable<>).MakeGenericType(valueType);
        var variable = (Variable)Activator.CreateInstance(variableGenericType)!;

        if (!string.IsNullOrEmpty(source.Id))
            variable.Id = source.Id;

        variable.Name = source.Name;

        if (!string.IsNullOrWhiteSpace(source.DefaultValue))
        {
            source.DefaultValue?.TryConvertTo(valueType).OnSuccess(value =>
            {
                variable.Value = value;
            }).OnFailure(ex =>
            {
                logger.LogWarning(ex, "Failed to convert the default value {DefaultValue} of variable {VariableName} to its type {VariableType}. Default value will not be set.", source.DefaultValue, source.Name, valueType);
            });
        }

        variable.StorageDriverType = GetStorageDriverType(source.StorageDriverType);

        return variable;
    }

    public IEnumerable<Variable> Map(IEnumerable<VariableDefinition>? source) =>
        source?
            .Select(Map)
            .Where(x => x != null)
            .Select(x => x!)
        ?? [];

    public VariableDefinition Map(Variable source)
    {
        var variableType = source.GetType();
        var valueType = variableType.IsConstructedGenericType ? variableType.GetGenericArguments().FirstOrDefault() ?? typeof(object) : typeof(object);
        var valueTypeAlias = SerializationTypeResolver.TryGetAlias(workflowJsonTypeRegistry, valueType, out var alias) ? alias : null;
        var value = source.Value;
        var serializedValue = value.Format();

        // Handles the case where an alias exists for an array or collection type. E.g. byte[] -> ByteArray.
        if (valueTypeAlias != null && (valueType.IsArray || valueType.IsCollectionType()))
            return new(source.Id, source.Name, valueTypeAlias, false, serializedValue, source.StorageDriverType);

        var isArray = valueType.IsArray;
        var isCollection = valueType.IsCollectionType();
        var elementValueType = isArray ? valueType.GetElementType()! : isCollection ? valueType.GenericTypeArguments[0] : valueType;
        var elementTypeAlias = SerializationTypeResolver.TryGetAlias(workflowJsonTypeRegistry, elementValueType, out var elementAlias)
            ? elementAlias
            : elementValueType.GetSimpleAssemblyQualifiedName();

        return new(source.Id, source.Name, elementTypeAlias, isArray, serializedValue, source.StorageDriverType);
    }

    public IEnumerable<VariableDefinition> Map(IEnumerable<Variable>? source) => source?.Select(Map) ?? [];

    private string? GetStorageDriverType(string? storageDriverTypeName)
    {
        if (string.IsNullOrEmpty(storageDriverTypeName))
            return null;

        // TODO: The following code handles backward compatibility with variable definitions referencing older .NET type namespaces.
        // We will refactor this by storing a driver identifier rather than its full type name - which is brittle in case we move namespaces.
        using var scope = scopeFactory.CreateScope();
        var storageDriverManager = scope.ServiceProvider.GetRequiredService<IStorageDriverManager>();

        return storageDriverManager.Find(storageDriverTypeName) != null ? storageDriverTypeName : null;
    }
}
