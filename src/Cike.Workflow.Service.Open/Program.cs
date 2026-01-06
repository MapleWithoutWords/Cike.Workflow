using Cike.AspNetCore.MinimalAPIs.Extensions;
using Cike.Core.Extensions;
using Cike.Workflow.Service.Open;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/logs.log", outputTemplate: "{Timestamp:HH:mm} || {Level} || {SourceContext:l} || {Message} || {Exception} ||end {NewLine}", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Logging.AddSerilog();

await builder.Services.AddApplicationAsync<CikeWorkflowServiceOpenModule>();

var app = builder.Build();

//app.UseMultiTenant();
await app.InitializeApplicationAsync();

app.MapGet("/", async context => context.Response.Redirect("/swagger"));

await app.RunAsync();
