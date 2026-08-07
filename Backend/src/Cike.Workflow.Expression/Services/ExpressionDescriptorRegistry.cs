namespace Cike.Workflow.Expressions.Services;

/// <inheritdoc />
internal class ExpressionDescriptorRegistry : IExpressionDescriptorRegistry
{
    private readonly IDictionary<string, ExpressionDescriptor> _expressionSyntaxDescriptors = new Dictionary<string, ExpressionDescriptor>();

    /// <summary>
    /// Represents a registry of expression descriptors.
    /// </summary>
    public ExpressionDescriptorRegistry()
    {
    }

    public static ExpressionDescriptorRegistry CreateDefault()
    {
        return new ExpressionDescriptorRegistry();
    }

    /// <inheritdoc />
    public void Add(ExpressionDescriptor descriptor)
    {
        _expressionSyntaxDescriptors[descriptor.Type] = descriptor;
    }

    /// <inheritdoc />
    public void AddRange(IEnumerable<ExpressionDescriptor> descriptors)
    {
        foreach (var descriptor in descriptors)
            Add(descriptor);
    }

    /// <inheritdoc />
    public IEnumerable<ExpressionDescriptor> ListAll() => _expressionSyntaxDescriptors.Values;

    /// <inheritdoc />
    public ExpressionDescriptor? Find(Func<ExpressionDescriptor, bool> predicate) => _expressionSyntaxDescriptors.Values.FirstOrDefault(predicate);

    /// <inheritdoc />
    public ExpressionDescriptor? Find(string type) => _expressionSyntaxDescriptors.TryGetValue(type, out var descriptor) ? descriptor : default;
}
