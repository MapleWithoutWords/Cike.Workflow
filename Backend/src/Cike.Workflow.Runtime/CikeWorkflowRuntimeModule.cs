using Cike.Locks.Abstracts;

namespace Cike.Workflow.Runtime;

[DependsOn([
    typeof(CikeWorkflowDomainModule),
    typeof(CikeWorkflowApplicationContractsModule),
    typeof(CikeWorkflowCachingModule),
    typeof(CikeLocksModule),
    ])]
public class CikeWorkflowRuntimeModule : CikeModule
{

}
