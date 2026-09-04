using Microsoft.Extensions.Options;

namespace Cike.Workflow.Common.Serialization.Internals;

/// <inheritdoc />
public class SerializationTypeRegistry : ISerializationTypeRegistry
{
    private readonly Dictionary<string, Type> _aliasTypeDictionary = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Type, string> _typeAliasDictionary = new();

    /// <summary>
    /// Creates a default registry.
    /// </summary>
    public static ISerializationTypeRegistry CreateDefault()
    {
        return new SerializationTypeRegistry(Options.Create(new SerializationTypeOptions()));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationTypeRegistry"/> class.
    /// </summary>
    public SerializationTypeRegistry(IOptions<SerializationTypeOptions> options)
    {
        foreach (var entry in options.Value.AliasTypeDictionary)
            RegisterTypeName(entry.Value, entry.Key);

        foreach (var entry in options.Value.TypeAliasDictionary)
            RegisterType(entry.Key, entry.Value);
    }

    /// <inheritdoc />
    public void RegisterType(Type type, string alias)
    {
        _typeAliasDictionary[type] = alias;
        RegisterTypeName(type, alias);
    }

    /// <inheritdoc />
    public bool TryGetAlias(Type type, out string alias) => _typeAliasDictionary.TryGetValue(type, out alias!);

    /// <inheritdoc />
    public bool TryGetType(string alias, out Type type) => _aliasTypeDictionary.TryGetValue(alias, out type!);

    /// <inheritdoc />
    public IEnumerable<Type> ListTypes() => _aliasTypeDictionary.Values.Distinct();

    private void RegisterTypeName(Type type, string alias)
    {
        _aliasTypeDictionary[alias] = type;

        if (type.IsPrimitive || type.IsValueType && Nullable.GetUnderlyingType(type) == null)
        {
            var nullableType = typeof(Nullable<>).MakeGenericType(type);
            _aliasTypeDictionary[$"{alias}?"] = nullableType;

            if (_typeAliasDictionary.ContainsKey(type))
                _typeAliasDictionary[nullableType] = $"{alias}?";
        }
    }

    public Type GetTypeOrDefault(string alias)
    {
        return TryGetType(alias, out var type) ? type : Type.GetType(alias) ?? typeof(object);
    }

    public string GetAliasOrDefault(Type type) =>
        TryGetAlias(type, out var alias) ? alias : type.GetSimpleAssemblyQualifiedName();

    public void RegisterType<T>(string alias)
    {
        RegisterType(typeof(T), alias);
    }
}
