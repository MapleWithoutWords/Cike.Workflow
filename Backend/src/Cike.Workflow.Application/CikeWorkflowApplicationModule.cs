using Cike.Workflow.Application.Contracts;

namespace Cike.Application;

[DependsOn([
    typeof(CikeDomainModule),
    typeof(CikeWorkflowApplicationContractsModule),
    typeof(CikeCqrsModule),
    typeof(CikeEventBusLocalModule),
    ])]
public class CikeWorkflowApplicationModule : CikeModule
{

}
