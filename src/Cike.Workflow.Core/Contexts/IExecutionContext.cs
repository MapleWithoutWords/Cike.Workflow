using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Memory;
using Elsa.Expressions.Models;

namespace Cike.Workflow.Core.Contexts;

public interface IExecutionContext
{
    string Id { get; }

    IActivity Activity { get; }

    ExpressionExecutionContext ExpressionExecutionContext { get; }
    
    IEnumerable<Variable> Variables { get; }
}
