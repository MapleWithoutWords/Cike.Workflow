namespace Cike.Application.Contracts;

[DependsOn(
    typeof(CikeWorkflowDomainSharedModule),
    typeof(CikeContractsModule)
)]
public class CikeWorkflowApplicationContractsModule : CikeModule
{

}
