using Cike.Workflow.Realtime.Models;

namespace Cike.Workflow.Realtime.Tests.Infrastructure;

/// <summary>
/// 推送探测器：以 NSubstitute 替身接管 IHubContext，捕获推往各组的事件与组订阅记录。
/// 由测试基座在模块加载前以 Singleton 注册，测试模块用工厂把它接管为 IHubContext 实现。
/// </summary>
public sealed class RealtimeHubProbe
{
    private readonly IHubContext<WorkflowRealtimeHub> _context;

    public RealtimeHubProbe()
    {
        var clients = Substitute.For<IHubClients>();
        var proxy = Substitute.For<IClientProxy>();

        clients.Group(Arg.Do<string>(group => Groups.Add(group))).Returns(proxy);

        proxy.SendCoreAsync(Arg.Any<string>(), Arg.Do<object?[]>(args =>
        {
            if (args is [WorkflowExecutionProgressEvent @event])
                Events.Add(@event);
        }), Arg.Any<CancellationToken>());

        var context = Substitute.For<IHubContext<WorkflowRealtimeHub>>();
        context.Clients.Returns(clients);
        _context = context;
    }

    /// <summary>按推送顺序记录的全部进度事件。</summary>
    public List<WorkflowExecutionProgressEvent> Events { get; } = [];

    /// <summary>发生过组订阅的组名（含重复，按发生顺序）。</summary>
    public List<string> Groups { get; } = [];

    /// <summary>接管 IHubContext 注册所用的替身实例。</summary>
    public IHubContext<WorkflowRealtimeHub> Context => _context;
}
