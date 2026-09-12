using System.Net.Http.Json;

namespace Cike.Workflow.Service.Open.Tests;

internal class ProbeTest : BaseIntegrationTest
{
    [Test]
    public async Task ProbeDb()
    {
        var ws = await CreateClient().PostAsJsonAsync("/api/v1/Workspaces", new { name = "probe-ws", description = "probe" });
        TestContext.Out.WriteLine($"WS STATUS: {ws.StatusCode}");
        TestContext.Out.WriteLine($"WS BODY: {await ws.Content.ReadAsStringAsync()}");
    }
}
