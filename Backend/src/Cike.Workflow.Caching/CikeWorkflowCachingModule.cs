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
        // 开放泛型无法按约定注册，手动补上；WorkflowDefinitionCache 等封闭实现走 IScopedDependency 约定注册
        context.Services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ICacheService<>), typeof(BaseCacheService<>)));

        await base.ConfigureServicesAsync(context);
    }
}
