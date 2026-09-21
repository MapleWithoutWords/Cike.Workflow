using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Domain.Shared.ValueObjects;

namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

/// <summary>
/// 画布校验请求：与发布相同的画布负载（根活动 + 选项），但不落库、不产生版本。
/// </summary>
public class ValidateWorkflowCanvasDto
{
    public IActivity Root { get; set; } = null!;

    public WorkflowDefinitionOptionsValueObject Options { get; set; } = new();
}
