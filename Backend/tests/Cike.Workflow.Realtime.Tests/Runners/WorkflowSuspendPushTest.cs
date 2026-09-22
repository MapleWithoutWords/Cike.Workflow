using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Realtime.Models;

namespace Cike.Workflow.Realtime.Tests.Runners;

/// <summary>
/// 测试用挂起活动：执行时创建书签后返回，实例随之进入挂起等待。
/// </summary>
public class SuspendingTestActivity : Activity
{
    protected override ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        context.CreateBookmark("resume");
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// 挂起推送集成测试：书签挂起节点推 ActivitySuspended，实例推 WorkflowSuspended 终态。
/// </summary>
[Category("Integration")]
[TestFixture]
public class WorkflowSuspendPushTest : RealtimeCoreTestBase
{
    [Test]
    public async Task RunAsync_WhenActivitySuspendsOnBookmark_PushesActivitySuspendedAndWorkflowSuspended()
    {
        var workflow = new WorkflowActivity(new Sequence
        {
            Activities = { new WriteLine("Before"), new SuspendingTestActivity() }
        });

        var result = await Runner.RunAsync(workflow);
        var instanceId = result.WorkflowExecutionContext.Id;

        Assert.That(Probe.Events[^1].Type, Is.EqualTo(WorkflowExecutionProgressType.WorkflowSuspended));
        Assert.That(Probe.Events, Has.All.Property("WorkflowInstanceId").EqualTo(instanceId));

        var suspended = Probe.Events.Single(e => e.Type == WorkflowExecutionProgressType.ActivitySuspended);
        Assert.That(suspended.ActivityNodeId, Does.Contain("SuspendingTestActivity"));
        var startedEvent = Probe.Events.Single(e =>
            e.Type == WorkflowExecutionProgressType.ActivityStarted && e.ActivityNodeId == suspended.ActivityNodeId);
        Assert.That(suspended.ActivityInstanceId, Is.EqualTo(startedEvent.ActivityInstanceId));
    }
}
