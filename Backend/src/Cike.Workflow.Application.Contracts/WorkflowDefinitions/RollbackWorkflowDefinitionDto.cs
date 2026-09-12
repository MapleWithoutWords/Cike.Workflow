namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class RollbackWorkflowDefinitionDto
{
    /// <summary>回滚目标版本号。</summary>
    public int TargetVersion { get; set; }
}
