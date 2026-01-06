using Cike.AspNetCore.MinimalAPIs;
using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Management.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cike.Workflow.Service.Open.Services;

public class WorkflowDefineService:MinimalApiServiceBase
{
    public WorkflowDefineService()
    {
        RouteOptions.RouteHandlerBuilder = null;
    }

    public async Task<IEnumerable<WorkflowDefinitionSummary>> GetPagedListAsync([FromServices] IWorkflowDefinitionStore store)
    {
      var list=await  store.FindSummariesAsync(new WorkflowDefinitionFilter());
        return list;
    }
}
