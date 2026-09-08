using Cike.Contracts.EntityDtos;
using Cike.Workflow.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class WorkflowDefinitionItemDto : AuditedEntityDto<long>
{
    public long FolderId { get; set; }

    public string DefinitionId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public WorkflowDefinitionType Type { get; set; }

    public bool UsableAsActivity { get; set; }

    public int Version { get; set; }

    public bool IsLatest { get; set; }

    public bool IsPublished { get; set; }

    public int? PublishedVersion { get; set; }

    public bool IsReadonly { get; set; }

    public bool IsSystem { get; set; }

    public string MaterializerName { get; set; } = null!;
}
