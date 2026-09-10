namespace Cike.Workflow.Application.WorkflowDefinitions.Queries;

public record GetWorkflowDefinitionVersionListQuery(string DefinitionId) : Query<List<WorkflowDefinitionVersionItemDto>>;
