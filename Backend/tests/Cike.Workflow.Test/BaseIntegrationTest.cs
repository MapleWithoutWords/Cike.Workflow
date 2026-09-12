using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Test;

public abstract class BaseIntegrationTest : IDisposable
{
    protected IServiceProvider serviceProvider;
    protected IServiceProvider _rootServices;
    private WebApplicationFactory<Program> _app;
    private IServiceScope _scope;

    public BaseIntegrationTest()
    {
        _app = new WebApplicationFactory<Program>()
          .WithWebHostBuilder(builder =>
          {
              builder.ConfigureServices((context, services) =>
              {
                  // 端点默认 RequireAuthorization，测试环境注册一个自动通过的认证方案
                  services.AddAuthorization();
                  services.AddAuthentication(TestAuthHandler.SchemeName)
                      .AddScheme<TestAuthOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
              });
          });

        _scope = _app.Services.CreateScope();
        serviceProvider = _scope.ServiceProvider;
        _rootServices = _app.Services;
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
