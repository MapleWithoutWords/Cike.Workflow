namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class RollbackWorkflowDefinitionDto
{
    /// <summary>工作流编号（DefinitionId 聚合）。</summary>
    public string DefinitionId { get; set; } = null!;

    /// <summary>回滚目标版本行 Id。</summary>
    public long DefinitionVersionId { get; set; }
}
