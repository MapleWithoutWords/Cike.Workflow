namespace Cike.Workflow.Expressions;

public interface IExpressionDescriptorRegistry
{
    void Add(ExpressionDescriptor descriptor);

    void AddRange(IEnumerable<ExpressionDescriptor> descriptors);

    IEnumerable<ExpressionDescriptor> ListAll();

    ExpressionDescriptor? Find(Func<ExpressionDescriptor, bool> predicate);

    ExpressionDescriptor? Find(string type);
}
