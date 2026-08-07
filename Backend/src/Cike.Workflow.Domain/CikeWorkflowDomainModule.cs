using Cike.Workflow.Core;

namespace Cike.Domain;

[DependsOn([
    typeof(CikeWorkflowDomainSharedModule),
    typeof(CikeCachingModule),
    typeof(CikeDomainModule),
    typeof(CikeWorkflowCoreModule),
    ])]
public class CikeWorkflowDomainModule : CikeModule
{

}
