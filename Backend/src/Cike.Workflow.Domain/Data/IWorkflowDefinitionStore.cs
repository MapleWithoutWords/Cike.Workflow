using System.Linq.Expressions;

namespace Cike.Workflow.Domain.Data;

public interface IWorkflowDefinitionStore : IBaseStore<WorkflowDefinition>
{
    Task<List<WorkflowDefinition>> GetListAsync(Expression<Func<WorkflowDefinition, bool>> filter, CancellationToken cancellationToken = default);
}
