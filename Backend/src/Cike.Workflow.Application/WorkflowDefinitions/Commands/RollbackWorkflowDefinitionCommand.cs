namespace Cike.Workflow.Application.WorkflowDefinitions.Commands;

/// <summary>
/// 回滚工作流定义到指定历史版本：有未发布草稿时用目标版本画布内容覆盖草稿（版本号不变）；
/// 最新版已发布时生成 v+1 新草稿、内容复制自目标版本。只回滚画布内容，元数据保持当前值。
/// </summary>
public record RollbackWorkflowDefinitionCommand(long Id, int TargetVersion) : Command;
