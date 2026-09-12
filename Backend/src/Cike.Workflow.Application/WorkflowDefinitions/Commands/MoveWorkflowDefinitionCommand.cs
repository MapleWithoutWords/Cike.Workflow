namespace Cike.Workflow.Application.WorkflowDefinitions.Commands;

/// <summary>
/// 移动工作流定义到同工作空间的其他目录：该 DefinitionId 的所有版本行 FolderId 一起更新。
/// </summary>
public record MoveWorkflowDefinitionCommand(long Id, long FolderId) : Command;
