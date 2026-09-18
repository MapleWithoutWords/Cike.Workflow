using Cike.Workflow.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Runtime.Models;

public class DispatchWorkflowInstanceRequest
{
    public DispatchWorkflowInstanceRequest()
    {
    }

    public DispatchWorkflowInstanceRequest(long instanceId)
    {
        InstanceId = instanceId;
    }

    public long InstanceId { get; init; }

    public long? BookmarkId { get; set; }

    public ActivityHandle? ActivityHandle { get; init; }

    public IDictionary<string, object>? Input { get; init; }

    public IDictionary<string, object>? Properties { get; init; }

    public string? CorrelationId { get; init; }
}
