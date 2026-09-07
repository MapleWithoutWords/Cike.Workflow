using Cike.Core.Modularity;
using Cike.Uow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Cike.Workflow.Core.Tests;

[DependsOn([
    typeof(CikeWorkflowCoreModule)
    ])]
internal class CikeWorkflowCoreTestModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddLogging();
        context.Services.AddSingleton(Substitute.For<IUnitOfWork>());
        await base.ConfigureServicesAsync(context);
    }
}
