namespace Cike.Workflow.Application.WorkflowDefinitions.Commands;

/// <summary>
/// 保存画布草稿：最新行未发布时就地覆盖内容；最新行已发布时生成 v+1 新草稿（版本号增长的唯一路径）。
/// </summary>
public record SaveWorkflowDefinitionCommand(long Id, SaveWorkflowDefinitionDto Dto) : Command
{
    /// <summary>保存后持有草稿的行 Id（已发布场景为新草稿行）。</summary>
    public long DraftId { get; set; }
}
