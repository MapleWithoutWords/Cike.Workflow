namespace Cike.Workflow.Core.Contexts;

public interface IExecutionContext
{
    long Id { get; }

    IActivity Activity { get; }

    ExpressionExecutionContext ExpressionExecutionContext { get; }

    IEnumerable<Variable> Variables { get; }

    public IDictionary<string, object> Properties { get; }
}
