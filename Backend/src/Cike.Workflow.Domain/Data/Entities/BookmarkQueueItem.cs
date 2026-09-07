using Cike.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Domain.Data.Entities;

public class BookmarkQueueItem : AuditedEntity<long>
{
    public long WorkflowInstanceId { get; set; }

    public string CorrelationId { get; set; } = null!;

    public long BookmarkId { get; set; }

    public string StimulusHash { get; set; } = string.Empty;

    public long ActivityInstanceId { get; set; }

    public string ActivityTypeName { get; set; } = string.Empty;

    public ResumeBookmarkOptions? Options { get; set; }

    public string? SerializedOptions { get; set; }
}
