using Cike.Workflow.Common.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Runtime;

public interface IWorkflowCancellationService
{
    Task<bool> CancelWorkflowAsync(long workflowInstanceId, CancellationToken cancellationToken = default);

    Task<int> CancelWorkflowsAsync(IEnumerable<long> workflowInstanceIds, CancellationToken cancellationToken = default);

    Task<int> CancelWorkflowByDefinitionVersionAsync(long definitionVersionId, CancellationToken cancellationToken = default);

    Task<int> CancelWorkflowByDefinitionAsync(string definitionId, VersionOptions versionOptions, CancellationToken cancellationToken = default);

}
