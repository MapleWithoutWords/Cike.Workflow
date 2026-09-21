namespace Cike.Workflow.Core.Validation;

/// <summary>
/// 画布严格校验触发器（发布前置校验）。各活动的校验规则由活动自己声明（覆写 Activity 的 Validate），
/// 容器类活动负责递归自己的子活动；触发器只做三件事：前置判断画布根类型、触发根活动自校验、校验变量定义。
/// </summary>
public interface IWorkflowValidator
{
    List<WorkflowValidationError> Validate(WorkflowValidationContext context);
}

/// <summary>校验输入：画布根活动 + 变量定义。上下文为单次校验专用，错误收集在其 Errors 中。</summary>
public record WorkflowValidationContext(IActivity Root, IReadOnlyList<WorkflowVariableDefinition> Variables)
{
    /// <summary>校验错误收集器：活动自校验时往里追加错误，触发器原样返回。</summary>
    public List<WorkflowValidationError> Errors { get; } = [];
}

/// <summary>Core 层的变量定义（定义实体的 Options.Variables 映射而来，避免 Core 反向依赖领域层）。</summary>
public record WorkflowVariableDefinition(string Id, string Name, string TypeName, bool IsArray);

/// <summary>
/// 校验错误：ActivityId 定位到画布上的活动（与活动无关的错误为 null，如变量定义）；
/// NodeId / Name 从被定位活动上原样回显（活动不存在时为 null），供前端错误列表展示与下钻定位。
/// </summary>
public record WorkflowValidationError(string? ActivityId, string? NodeId, string? Name, string Message)
{
    /// <summary>活动自报家门：从活动实例上取 ActivityId / NodeId / Name 构造错误。</summary>
    public WorkflowValidationError(IActivity activity, string Message) : this(activity.Id, activity.NodeId, activity.Name, Message)
    {
    }
}
