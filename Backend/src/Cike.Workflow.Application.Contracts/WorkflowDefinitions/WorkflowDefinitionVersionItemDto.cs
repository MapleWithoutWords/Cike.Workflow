using Cike.Contracts.EntityDtos;

namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class WorkflowDefinitionVersionItemDto : AuditedEntityDto<long>
{
    public int Version { get; set; }

    public bool IsLatest { get; set; }

    public bool IsPublished { get; set; }

    public string PublishedNote { get; set; } = string.Empty;

    public long PublishedBy { get; set; } = 0;

    public DateTime PublishedAt { get; set; } = default;
}
