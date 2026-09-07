using Cike.Data;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Domain.Data.Entities;

public class WorkflowInstance : FullAuditedAggregateRoot<long>, IMultiTenant
{
    public long TenantId { get; set; }

    public string DefinitionId { get; set; } = null!;

    public long DefinitionVersionId { get; set; }

    public int Version { get; set; }

    public long ParentWorkflowInstanceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;

    public bool IsExecuting { get; set; }

    public int IncidentCount { get; set; }

    public WorkflowStatus Status { get; set; }

    public DateTime FinishedAt { get; set; }

    public WorkflowState WorkflowState { get; set; } = null!;
}
