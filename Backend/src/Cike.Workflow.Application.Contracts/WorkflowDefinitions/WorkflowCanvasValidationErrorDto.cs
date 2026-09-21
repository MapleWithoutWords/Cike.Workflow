namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

/// <summary>
/// 画布校验错误：ActivityId 可定位到画布节点（与节点无关的错误为 null，如变量定义）；
/// NodeId / Name 从被定位活动上原样回显，供前端错误列表展示与下钻定位。
/// </summary>
public class WorkflowCanvasValidationErrorDto
{
    public string? ActivityId { get; set; }

    public string? NodeId { get; set; }

    public string? Name { get; set; }

    public string Message { get; set; } = null!;
}
