using Cike.Workflow.Core.Activities.Abstracts;

namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class WorkflowDefinitionDetailDto : AuditedEntityDto<long>
{
    public string DefinitionId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public WorkflowDefinitionType Type { get; set; }

    public bool UsableAsActivity { get; set; }

    public string MaterializerName { get; set; } = null!;

    public IActivity Root { get; set; } = null!;

    public WorkflowDefinitionOptionsValueObject Options { get; set; } = new();

    public bool IsReadonly { get; set; }

    public bool IsSystem { get; set; }

    public int Version { get; set; }

    public bool IsLatest { get; set; }

    public bool IsPublished { get; set; }

    public long FolderId { get; set; }
}
