using Cike.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Tests;

public abstract class BaseIntegrationTest
{
    protected IServiceProvider serviceProvider;

    public BaseIntegrationTest()
    {
        var services = new ServiceCollection();
        services.AddApplicationAsync<CikeWorkflowCoreTestModule>();
        serviceProvider = services.BuildServiceProvider();

    }
}
