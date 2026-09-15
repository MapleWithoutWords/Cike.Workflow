namespace Cike.Workflow.Runtime;

[DependsOn([
    typeof(CikeWorkflowDomainModule),
    typeof(CikeWorkflowApplicationContractsModule),
    typeof(CikeWorkflowCachingModule),
    ])]
public class CikeWorkflowRuntimeModule : CikeModule
{

}
