namespace Cike.Workflow.Application.Contracts.Workspaces;

public class WorkspaceItemDto : AuditedEntityDto<long>
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
}
