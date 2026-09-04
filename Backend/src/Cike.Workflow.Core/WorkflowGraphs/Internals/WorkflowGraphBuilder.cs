using Cike.Workflow.Core.WorkflowGraphs.Models;

namespace Cike.Workflow.Core.WorkflowGraphs.Internals;

public class WorkflowGraphBuilder(IActivityVisitor activityVisitor, IIdentityGraphService identityGraphService) : IWorkflowGraphBuilder, IScopedDependency
{
    /// <inheritdoc />
    public async Task<WorkflowGraph> BuildAsync(WorkflowActivity workflow, CancellationToken cancellationToken = default)
    {
        var graph = await activityVisitor.VisitAsync(workflow, cancellationToken);
        var nodes = graph.Flatten().ToList();

        await identityGraphService.AssignIdentitiesAsync(nodes);
        return new WorkflowGraph(workflow, graph, nodes);
    }
}
