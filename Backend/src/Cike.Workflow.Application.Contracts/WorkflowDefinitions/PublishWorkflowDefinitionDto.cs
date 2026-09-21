using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Domain.Shared.ValueObjects;

namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class PublishWorkflowDefinitionDto
{
    public IActivity Root { get; set; } = null!;

    public WorkflowDefinitionOptionsValueObject Options { get; set; } = new();

    public string? PublishedNote { get; set; }
}
