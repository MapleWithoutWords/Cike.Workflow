namespace Cike.Workflow.Application.WorkflowDefinitions.Commands;

/// <summary>
/// 发布工作流定义：只能发布最新行，就地打 IsPublished 标记并记录备注/发布人/时间。
/// </summary>
public record PublishWorkflowDefinitionCommand(long Id, string? PublishedNote) : Command;
