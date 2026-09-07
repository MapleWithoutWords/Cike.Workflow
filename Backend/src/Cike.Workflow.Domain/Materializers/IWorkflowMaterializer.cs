namespace Cike.Workflow.Domain.Materializers;

public interface IWorkflowMaterializer
{
    string Name { get; }

    ValueTask<WorkflowActivity> MaterializeAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default);
}
