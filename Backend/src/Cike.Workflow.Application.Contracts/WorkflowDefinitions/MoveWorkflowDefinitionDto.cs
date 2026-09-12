namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class MoveWorkflowDefinitionDto
{
    /// <summary>目标目录 Id，0 表示根目录。</summary>
    public long FolderId { get; set; }
}
