using Cike.Workflow.Application.Contracts;
using Cike.Workflow.Caching;

namespace Cike.Application;

[DependsOn([
    typeof(CikeDomainModule),
    typeof(CikeWorkflowApplicationContractsModule),
    typeof(CikeCqrsModule),
    typeof(CikeEventBusLocalModule),
    typeof(CikeWorkflowCachingModule),
    ])]
public class CikeWorkflowApplicationModule : CikeModule
{

}
