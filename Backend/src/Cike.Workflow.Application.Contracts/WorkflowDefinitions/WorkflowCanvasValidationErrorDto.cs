namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

/// <summary>
/// 画布校验错误：ActivityId 可定位到画布节点（与节点无关的错误为 null，如变量定义）。
/// </summary>
public class WorkflowCanvasValidationErrorDto
{
    public string? ActivityId { get; set; }

    public string Message { get; set; } = null!;
}
