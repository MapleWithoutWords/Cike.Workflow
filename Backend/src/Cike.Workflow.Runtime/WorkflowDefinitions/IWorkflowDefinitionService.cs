using Cike.Workflow.Common.Versions;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.WorkflowGraphs.Models;

namespace Cike.Workflow.Runtime.WorkflowDefintions;

public interface IWorkflowDefinitionService
{
    Task<WorkflowGraph> MaterializeWorkflowAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default);

    Task<WorkflowGraph?> FindWorkflowGraphAsync(long definitionVersionId, CancellationToken cancellationToken = default);

    Task<WorkflowDefinition?> FindWorkflowDefinitionAsync(string definitionId, VersionOptions versionOptions, CancellationToken cancellationToken = default);

    Task<WorkflowDefinition?> FindWorkflowDefinitionAsync(long definitionVersionId, CancellationToken cancellationToken = default);

    Task<WorkflowDefinition?> FindWorkflowDefinitionAsync(WorkflowDefinitionHandle handle, CancellationToken cancellationToken = default);

    Task<WorkflowGraph> GetWorkflowGraphAsync(WorkflowDefinitionHandle definitionHandle, CancellationToken cancellationToken = default);
}
