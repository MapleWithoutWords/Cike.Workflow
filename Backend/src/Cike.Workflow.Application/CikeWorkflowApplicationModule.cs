using Cike.Workflow.Application.Contracts;
using Cike.Workflow.Caching;
using Cike.Workflow.Runtime;

namespace Cike.Application;

[DependsOn([
    typeof(CikeDomainModule),
    typeof(CikeWorkflowApplicationContractsModule),
    typeof(CikeCqrsModule),
    typeof(CikeEventBusLocalModule),
    typeof(CikeWorkflowCachingModule),

    typeof(CikeWorkflowRuntimeModule),
    ])]
public class CikeWorkflowApplicationModule : CikeModule
{

}
