namespace Cike.Workflow.Core.StorageDrivers.Internals;

internal class StorageDriverRegistry : IStorageDriverRegistry
{
    private readonly IDictionary<string, StorageDriverDescriptor> _expressionSyntaxDescriptors = new Dictionary<string, StorageDriverDescriptor>();

    /// <summary>
    /// Represents a registry of expression descriptors.
    /// </summary>
    public StorageDriverRegistry()
    {
    }

    public static StorageDriverRegistry CreateDefault()
    {
        return new StorageDriverRegistry();
    }

    /// <inheritdoc />
    public void Add(StorageDriverDescriptor descriptor)
    {
        _expressionSyntaxDescriptors[descriptor.Type] = descriptor;
    }

    /// <inheritdoc />
    public void AddRange(IEnumerable<StorageDriverDescriptor> descriptors)
    {
        foreach (var descriptor in descriptors)
            Add(descriptor);
    }

    /// <inheritdoc />
    public IEnumerable<StorageDriverDescriptor> ListAll() => _expressionSyntaxDescriptors.Values;

    /// <inheritdoc />
    public StorageDriverDescriptor? Find(Func<StorageDriverDescriptor, bool> predicate) => _expressionSyntaxDescriptors.Values.FirstOrDefault(predicate);

    /// <inheritdoc />
    public StorageDriverDescriptor? Find(string type) => _expressionSyntaxDescriptors.TryGetValue(type, out var descriptor) ? descriptor : default;
}
