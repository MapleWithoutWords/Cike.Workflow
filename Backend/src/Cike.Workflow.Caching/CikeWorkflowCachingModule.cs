using Cike.Workflow.Caching.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cike.Workflow.Caching;

[DependsOn([
    typeof(CikeCachingModule)
    ])]
public class CikeWorkflowCachingModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ICacheService<>), typeof(BaseCacheService<>)));

        await base.ConfigureServicesAsync(context);
    }
}
