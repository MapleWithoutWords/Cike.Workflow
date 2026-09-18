namespace Cike.Workflow.Application.WorkflowInstances.Queries;

public record GetWorkflowInstanceQuery(long Id) : Query<WorkflowInstanceDetailDto>;
