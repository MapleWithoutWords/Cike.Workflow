using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Domain.Shared.ValueObjects;

namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class SaveWorkflowDefinitionDto
{
    public IActivity Body { get; set; } = null!;

    public WorkflowDefinitionOptionsValueObject Options { get; set; } = new();
}
