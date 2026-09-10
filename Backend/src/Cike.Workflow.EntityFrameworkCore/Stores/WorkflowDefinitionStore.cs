namespace Cike.EntityFrameworkCore.Stores;

public class WorkflowDefinitionStore(CikeWorkflowDbContenxt context)
    : BaseStore<WorkflowDefinition>(context), IWorkflowDefinitionStore
{
    public async Task<List<WorkflowDefinition>> GetListAsync(Expression<Func<WorkflowDefinition, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await GetListAsync(filter, "CreatedAt desc", cancellationToken);
    }
}
