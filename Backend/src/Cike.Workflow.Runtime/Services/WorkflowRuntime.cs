using Cike.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Runtime.Internals;

internal class WorkflowRuntime : IWorkflowRuntime, IScopedDependency
{
    public ValueTask<IWorkflowClient> CreateClientAsync(long? workflowInstanceId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
