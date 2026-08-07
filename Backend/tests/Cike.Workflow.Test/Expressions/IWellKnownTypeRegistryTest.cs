using Cike.Workflow.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Test.Expressions;

internal class IWellKnownTypeRegistryTest : BaseIntegrationTest
{
    [Test]
    public async Task GetAsync()
    {
        var wellKnownTypeRegistry = serviceProvider.GetRequiredService<IWellKnownTypeRegistry>();
    }
}
