using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cike.Workflow.Service.Open.Tests;

/// <summary>
/// 测试用认证处理器：所有请求自动通过，携带固定用户声明（PublishedBy 等审计字段断言依赖它）。
/// </summary>
public class TestAuthHandler(
    IOptionsMonitor<TestAuthOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<TestAuthOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestScheme";
    public const string TestUserId = "00000000-0000-0000-0000-000000000001";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, TestUserId),
            new(ClaimTypes.Name, "test-user"),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class TestAuthOptions : AuthenticationSchemeOptions
{
}
