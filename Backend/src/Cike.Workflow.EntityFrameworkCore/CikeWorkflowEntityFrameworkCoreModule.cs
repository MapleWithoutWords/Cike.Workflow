using Cike.Data.Extensions;

namespace Cike.EntityFrameworkCore;

[DependsOn([
    typeof(CikeWorkflowDomainModule),
    typeof(CikeDataEFCoreMySqlModule),
    typeof(CikeWorkflowCachingModule),
    ])]
public class CikeWorkflowEntityFrameworkCoreModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddCikeDbContext<CikeWorkflowDbContenxt>();
        await base.ConfigureServicesAsync(context);
    }
}
