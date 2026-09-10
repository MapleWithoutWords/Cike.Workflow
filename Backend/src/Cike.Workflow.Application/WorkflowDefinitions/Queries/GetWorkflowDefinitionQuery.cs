namespace Cike.Workflow.Application.WorkflowDefinitions.Queries;

public record GetWorkflowDefinitionQuery(long Id) : Query<WorkflowDefinitionDetailDto>
{
}
