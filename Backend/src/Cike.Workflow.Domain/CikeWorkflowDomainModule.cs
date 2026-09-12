using Cike.Workflow.Core;
using Cike.Workflow.Domain.Shared;

namespace Cike.Domain;

[DependsOn([
    typeof(CikeWorkflowDomainSharedModule),
    typeof(CikeCachingModule),
    typeof(CikeDomainModule),
    typeof(CikeWorkflowCoreModule),
    ])]
public class CikeWorkflowDomainModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        await base.ConfigureServicesAsync(context);
    }
}
