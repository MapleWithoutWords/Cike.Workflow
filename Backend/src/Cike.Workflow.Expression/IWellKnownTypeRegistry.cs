namespace Cike.Workflow.Expressions;

/// <summary>
/// A central repository of well known types.
/// </summary>
public interface IWellKnownTypeRegistry
{
    /// <summary>
    /// Register a type with an alias. 
    /// </summary>
    void RegisterType(Type type, string alias);

    /// <summary>
    /// Attempts to get an alias for the specified type.
    /// </summary>
    bool TryGetAlias(Type type, out string alias);

    /// <summary>
    /// Attempts to get the type associated with the specified alias.
    /// </summary>
    bool TryGetType(string alias, out Type type);

    /// <summary>
    /// Returns all registered types.
    /// </summary>
    IEnumerable<Type> ListTypes();

    /// <summary>
    /// Register type <typeparamref name="T"/> with the specified alias.
    /// </summary>
    public void RegisterType<T>(string alias);

    /// <summary>
    /// Attempt to return a type with the specified alias.
    /// </summary>
    public bool TryGetTypeOrDefault(string alias, out Type type);

    /// <summary>
    /// Returns the alias for the specified type. If no alias was found, the assembly qualified type name is returned instead.  
    /// </summary>
    public string GetAliasOrDefault(Type type);

    /// <summary>
    /// Returns the type associated with the specified alias. If no type was found, the alias is interpreted as a type name/
    /// </summary>
    public Type GetTypeOrDefault(string alias);
}
