namespace Cike.EntityFrameworkCore;

[DependsOn([
    typeof(CikeWorkflowDomainModule),
    typeof(CikeDataEFCoreMySqlModule),
    typeof(CikeWorkflowCachingModule),
    ])]
public class CikeWorkflowEntityFrameworkCoreModule : CikeModule
{

}
