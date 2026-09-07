using Cike.Core.Extensions;
using Cike.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Tests;

public abstract class BaseIntegrationTest
{
    protected IServiceProvider serviceProvider;
    protected DirectWorkflowRunner runner;

    public BaseIntegrationTest()
    {
        var services = new ServiceCollection();
        services.AddApplicationAsync<CikeWorkflowCoreTestModule>().GetAwaiter().GetResult();
        serviceProvider = services.BuildServiceProvider();

        var application = serviceProvider.GetRequiredService<IApplicationWithExternalServiceProvider>();
        application.InitializeAsync(serviceProvider).ConfigureAwait(false).GetAwaiter().GetResult();

        runner = new DirectWorkflowRunner(serviceProvider);
    }
}
