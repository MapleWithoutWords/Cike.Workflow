using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Domain.Shared.ValueObjects;

namespace Cike.Workflow.Application.WorkflowDefinitions.Commands;

/// <summary>
/// 发布工作流定义：携带完整画布（所见即所发），先严格画布校验再落库打标。
/// 最新行未发布时就地发布；已发布则生成 v+1 新草稿并直接发布。
/// </summary>
public record PublishWorkflowDefinitionCommand(
    long Id,
    IActivity Root,
    WorkflowDefinitionOptionsValueObject Options,
    string? PublishedNote) : Command
{
    /// <summary>发布后持有版本行的行 Id。</summary>
    public long PublishedId { get; set; }
}
