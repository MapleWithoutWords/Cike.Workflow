using Cike.Workflow.Core.Runners.Models;

namespace Cike.Workflow.Application.Contracts.WorkflowInstances;

public class WorkflowInstanceDetailDto : WorkflowInstanceItemDto
{
    public WorkflowState WorkflowState { get; set; } = null!;

    public List<ActivityInstanceExecutionRecordDto> ActivityInstances { get; set; } = [];
}
