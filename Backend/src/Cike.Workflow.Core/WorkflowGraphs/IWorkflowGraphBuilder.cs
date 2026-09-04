using Cike.Workflow.Core.WorkflowGraphs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.WorkflowGraphs;

public interface IWorkflowGraphBuilder
{
    /// <summary>
    /// Builds a workflow graph from a workflow.
    /// </summary>
    Task<WorkflowGraph> BuildAsync(WorkflowActivity workflow, CancellationToken cancellationToken = default);
}
