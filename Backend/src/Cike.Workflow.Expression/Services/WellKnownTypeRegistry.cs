namespace Cike.Workflow.Expressions.Services;

/// <inheritdoc />
public class WellKnownTypeRegistry : IWellKnownTypeRegistry
{
    private readonly Dictionary<string, Type> _aliasTypeDictionary = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Type, string> _typeAliasDictionary = new();

    public WellKnownTypeRegistry(IOptions<ExpressionOptions> expressionOptions)
    {
        foreach (var entry in expressionOptions.Value.AliasTypeDictionary)
            RegisterType(entry.Value, entry.Key);
    }

    /// <inheritdoc />
    public void RegisterType(Type type, string alias)
    {
        _typeAliasDictionary[type] = alias;
        _aliasTypeDictionary[alias] = type;

        if (type.IsPrimitive || type.IsValueType && Nullable.GetUnderlyingType(type) == null)
        {
            var nullableType = typeof(Nullable<>).MakeGenericType(type);
            var nullableAlias = alias + "?";
            _typeAliasDictionary[nullableType] = nullableAlias;
            _aliasTypeDictionary[nullableAlias] = nullableType;
        }
    }

    /// <inheritdoc />
    public bool TryGetAlias(Type type, out string alias) => _typeAliasDictionary.TryGetValue(type, out alias!);

    /// <inheritdoc />
    public bool TryGetType(string alias, out Type type) => _aliasTypeDictionary.TryGetValue(alias, out type!);

    /// <inheritdoc />
    public IEnumerable<Type> ListTypes() => _typeAliasDictionary.Keys;

    /// <summary>
    /// Register type <typeparamref name="T"/> with the specified alias.
    /// </summary>
    public void RegisterType<T>(string alias) => RegisterType(typeof(T), alias);

    /// <summary>
    /// Attempt to return a type with the specified alias.
    /// </summary>
    public bool TryGetTypeOrDefault(string alias, out Type type)
    {
        if (TryGetType(alias, out type))
            return true;

        var t = Type.GetType(alias);

        if (t == null)
            return false;

        type = t;
        return true;
    }

    /// <summary>
    /// Returns the alias for the specified type. If no alias was found, the assembly qualified type name is returned instead.  
    /// </summary>
    public string GetAliasOrDefault(Type type) =>
        TryGetAlias(type, out var alias) ? alias : type.GetSimpleAssemblyQualifiedName();

    /// <summary>
    /// Returns the type associated with the specified alias. If no type was found, the alias is interpreted as a type name/
    /// </summary>
    public Type GetTypeOrDefault(string alias) => TryGetType(alias, out var type) ? type : Type.GetType(alias) ?? typeof(object);
}
