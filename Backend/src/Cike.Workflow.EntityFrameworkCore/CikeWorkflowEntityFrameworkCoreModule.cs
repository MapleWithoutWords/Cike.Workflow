namespace Cike.EntityFrameworkCore;

[DependsOn([
    typeof(CikeWorkflowDomainModule),
    typeof(CikeDataEFCoreMySqlModule),
    ])]
public class CikeWorkflowEntityFrameworkCoreModule : CikeModule
{

}
