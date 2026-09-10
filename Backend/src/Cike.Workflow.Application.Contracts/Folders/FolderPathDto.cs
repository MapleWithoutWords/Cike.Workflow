namespace Cike.Workflow.Application.Contracts.Folders;

public class FolderPathDto : EntityDto<long>
{
    public string Name { get; set; } = null!;
}
