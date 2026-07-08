using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Test;

public abstract class BaseIntegrationTest : IDisposable
{
    protected IServiceProvider serviceProvider;
    private WebApplicationFactory<Program> _app;
    private IServiceScope _scope;
 
    public BaseIntegrationTest()
    {
        _app = new WebApplicationFactory<Program>()
          .WithWebHostBuilder(builder =>
          {
              builder.ConfigureServices((context, services) =>
              {
              });
          });
        
        _scope = _app.Services.CreateScope();
        serviceProvider = _scope.ServiceProvider;
    }

    protected HttpClient CreateClient() => _app.CreateClient();

    public void Dispose()
    {
        try
        {
            _scope?.Dispose();
            _app?.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Ignore disposed exception during teardown
        }
    }
}
