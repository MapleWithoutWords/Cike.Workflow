using Cike.Workflow.Domain.Filters;

namespace Cike.Workflow.Application.WorkflowInstances.Queries;

public record GetPagedWorkflowInstanceListQuery(WorkflowInstanceFilter Filter, PagedAndSortedResultRequest PageDto)
    : Query<PagedResultDto<WorkflowInstanceItemDto>>;
