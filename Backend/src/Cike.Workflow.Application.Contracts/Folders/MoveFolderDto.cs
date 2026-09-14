namespace Cike.Workflow.Application.Contracts.Folders;

public class MoveFolderDto
{
    /// <summary>目标上级目录 Id，0 表示移动到根目录。</summary>
    public long ParentId { get; set; }
}
