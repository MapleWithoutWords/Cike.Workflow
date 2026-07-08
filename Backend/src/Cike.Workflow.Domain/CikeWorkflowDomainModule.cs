namespace Cike.Domain;

[DependsOn([
    typeof(CikeWorkflowDomainSharedModule),
    typeof(CikeCachingModule),
    typeof(CikeDomainModule),
    ])]
public class CikeWorkflowDomainModule : CikeModule
{

}
