using Cike.EventBus.Local;
using Cike.EventBus.Local.LocalEventMiddlewares;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Runners.Internals.Commands;
using Cike.Workflow.Realtime.Internals;
using Cike.Workflow.Realtime.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Realtime.Tests.WorkflowInstances;

/// <summary>
/// 取消推送测试：取消不经过实例运行命令（取消 = 建上下文→发布取消命令→Cancel→存状态），
/// 由取消命令中间件单独补推终态。
/// 说明：Application→Runtime 的派发链当前是未实现存根（WorkflowRuntime.CreateClientAsync），
/// 端到端取消流走不通，故在此以直接调用中间件的方式锁定推送行为；
/// 派发链落地后中间件注册即自动生效，无需改动。
/// </summary>
[TestFixture]
public class CancelWorkflowPushTest : RealtimeCoreTestBase
{
    [Test]
    public async Task HandleAsync_WhenCancelCommandCompletes_PushesWorkflowCanceled()
    {
        var middleware = Services.GetRequiredService<ILocalEventMiddleware<RunCancelWorkflowCommand>>();
        var instanceId = 12345L;
        var context = await CreateContextAsync(instanceId);

        var next = Substitute.For<EventHandlerDelegate>();
        next.Invoke().Returns(Task.CompletedTask);

        await middleware.HandleAsync(new RunCancelWorkflowCommand(context), next);

        var canceled = Probe.Events
            .Where(e => e.Type == WorkflowExecutionProgressType.WorkflowCanceled)
            .ToList();
        Assert.That(canceled, Has.Count.EqualTo(1));
        Assert.That(canceled[0].WorkflowInstanceId, Is.EqualTo(instanceId));
        Assert.That(Probe.Groups, Does.Contain($"instance:{instanceId}"));
    }

    [Test]
    public void Resolve_WhenLoaded_CanResolveCancelMiddlewareRegistration()
    {
        // 注册即生效：Realtime 模块加载后，取消命令的推送中间件可从容器解析
        Assert.That(Services.GetRequiredService<ILocalEventMiddleware<RunCancelWorkflowCommand>>(),
            Is.InstanceOf<CancelWorkflowPushMiddleware>());
    }

    /// <summary>经真实图构建器构造一个带指定实例 Id 的执行上下文。</summary>
    private async Task<WorkflowExecutionContext> CreateContextAsync(long instanceId)
    {
        var graphBuilder = Services.GetRequiredService<IWorkflowGraphBuilder>();
        var graph = await graphBuilder.BuildAsync(new WorkflowActivity(new Sequence
        {
            Activities = { new WriteLine("Cancel Push Test") }
        }), default);
        return await WorkflowExecutionContext.CreateAsync(Services, graph, instanceId, default);
    }
}
