using Cike.Workflow.Realtime.Models;

namespace Cike.Workflow.Realtime.Tests.Runners;

/// <summary>
/// 运行推送序列集成测试：经真实 IWorkflowRunner 跑工作流，
/// 断言推往实例组的进度事件序列（节点开始/完成、实例终态）。
/// 引擎会对每个活动命令推送 Started；叶子节点完成后推 Completed；
/// 实例终态由工作流命令中间件按状态推送。
/// </summary>
[Category("Integration")]
[TestFixture]
public class WorkflowRunPushTest : RealtimeCoreTestBase
{
    private static WorkflowActivity BuildTwoStepSequence() => new(new Sequence
    {
        Activities = { new WriteLine("Hello"), new WriteLine("World") }
    });

    [Test]
    public async Task RunAsync_WithSequence_PushesStartCompletePairsPerNodeAndWorkflowFinished()
    {
        var result = await Runner.RunAsync(BuildTwoStepSequence());
        var instanceId = result.WorkflowExecutionContext.Id;

        Assert.That(Probe.Groups, Does.Contain($"instance:{instanceId}"));
        Assert.That(Probe.Events, Has.All.Property("WorkflowInstanceId").EqualTo(instanceId));
        Assert.That(Probe.Events[^1].Type, Is.EqualTo(WorkflowExecutionProgressType.WorkflowFinished));

        var started = Probe.Events.Where(e => e.Type == WorkflowExecutionProgressType.ActivityStarted).ToList();
        var completed = Probe.Events.Where(e => e.Type == WorkflowExecutionProgressType.ActivityCompleted).ToList();
        var startedNodeIds = started.Select(e => e.ActivityNodeId).ToList();
        var completedNodeIds = completed.Select(e => e.ActivityNodeId).ToList();

        // 叶子节点完成后成对出现，且都推送过开始事件
        Assert.That(completedNodeIds, Is.EqualTo(new[]
        {
            "Workflow1:Sequence1:WriteLine1", "Workflow1:Sequence1:WriteLine2",
        }));
        Assert.That(startedNodeIds, Does.Contain("Workflow1:Sequence1"));
        Assert.That(startedNodeIds, Does.Contain("Workflow1"));

        // 完成事件携带与开始事件一致的活动实例 Id
        foreach (var completedEvent in completed)
        {
            var startedEvent = started.Single(e => e.ActivityNodeId == completedEvent.ActivityNodeId);
            Assert.That(completedEvent.ActivityInstanceId, Is.EqualTo(startedEvent.ActivityInstanceId));
        }

        // 无失败推送
        Assert.That(Probe.Events, Has.None.Property("Type").EqualTo(WorkflowExecutionProgressType.ActivityFaulted));
        Assert.That(Probe.Events, Has.None.Property("Type").EqualTo(WorkflowExecutionProgressType.WorkflowFaulted));
    }

    [Test]
    public async Task RunAsync_WhenActivityFaults_PushesActivityFaulted()
    {
        // 说明：当前引擎在活动故障后的续延阶段会在 Faulted 状态上再次迁移并抛
        // "Cannot transition from Faulted to Faulted"（已向用户报告，待引擎侧修复）。
        // 这里先以该异常为前提，锁定 ActivityFaulted 推送行为；引擎修复后应改为
        // 正常断言运行结束并推送 WorkflowFaulted 终态。
        var workflow = new WorkflowActivity(new Sequence
        {
            Activities = { new WriteLine("Before"), Fault.Create("TestFault", "Test", "System", "boom") }
        });

        Assert.ThrowsAsync<Exception>(async () => await Runner.RunAsync(workflow));

        var faultedNodeIds = Probe.Events
            .Where(e => e.Type == WorkflowExecutionProgressType.ActivityFaulted)
            .Select(e => e.ActivityNodeId)
            .ToList();
        Assert.That(faultedNodeIds, Does.Contain("Workflow1:Sequence1:Fault1"));
        // 失败节点也推送过开始事件，且携带活动实例 Id
        var faulted = Probe.Events.First(e => e.Type == WorkflowExecutionProgressType.ActivityFaulted);
        var startedEvent = Probe.Events.Single(e => e.Type == WorkflowExecutionProgressType.ActivityStarted && e.ActivityNodeId == faulted.ActivityNodeId);
        Assert.That(faulted.ActivityInstanceId, Is.EqualTo(startedEvent.ActivityInstanceId));
    }
}
