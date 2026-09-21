using Cike.Workflow.Application.Contracts.WorkflowDefinitions;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Domain.Shared.ValueObjects;

namespace Cike.Workflow.Application.WorkflowDefinitions.Queries;

/// <summary>画布校验用例：走与发布相同的严格校验，返回结构化错误列表（空列表即通过）。</summary>
public record ValidateWorkflowCanvasQuery(IActivity Root, WorkflowDefinitionOptionsValueObject Options)
    : Query<List<WorkflowCanvasValidationErrorDto>>;
