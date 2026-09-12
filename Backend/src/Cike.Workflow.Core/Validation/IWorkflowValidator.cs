namespace Cike.Workflow.Core.Validation;

/// <summary>
/// 画布严格校验器（发布前置校验）。Core 拥有活动模型，校验规则随引擎演进。
/// 校验项：开始节点存在、无孤立节点、变量定义合法、活动必填属性齐全；错误信息可定位到具体节点/问题。
/// </summary>
public interface IWorkflowValidator
{
    List<WorkflowValidationError> Validate(WorkflowValidationContext context);
}

/// <summary>校验输入：画布根活动 + 变量定义。</summary>
public record WorkflowValidationContext(IActivity Root, IReadOnlyList<WorkflowVariableDefinition> Variables);

/// <summary>Core 层的变量定义（定义实体的 Options.Variables 映射而来，避免 Core 反向依赖领域层）。</summary>
public record WorkflowVariableDefinition(string Id, string Name, string TypeName, bool IsArray);

/// <summary>校验错误：NodeId 可定位到画布节点（与节点无关的错误为 null，如变量定义）。</summary>
public record WorkflowValidationError(string? NodeId, string Message);
