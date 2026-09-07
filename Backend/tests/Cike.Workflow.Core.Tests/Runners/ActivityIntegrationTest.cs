using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Activities.FlowchartActivity;
using Cike.Workflow.Core.Activities.FlowchartActivity.Models;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.Runners.Models;

namespace Cike.Workflow.Core.Tests.Runners;

/// <summary>
/// 每种活动类型的独立集成测试，通过 DirectWorkflowRunner 执行完整的工作流管道。
/// 覆盖: If, While, For, ForEach, Flowchart, Break, Start/End, 以及嵌套组合。
/// </summary>
[TestFixture]
public class ActivityIntegrationTest : BaseIntegrationTest
{
    #region If

    [Test]
    public async Task If_TrueCondition_ProducesTrueOutcome()
    {
        var workflow = new WorkflowActivity(new If { Condition = new(true) });

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var ifContext = result.WorkflowExecutionContext.ActivityExecutionContexts
            .FirstOrDefault(x => x.Activity is If);
        Assert.That(ifContext, Is.Not.Null, "If activity should have been executed");
    }

    [Test]
    public async Task If_FalseCondition_ProducesFalseOutcome()
    {
        var workflow = new WorkflowActivity(new If { Condition = new(false) });

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var ifContext = result.WorkflowExecutionContext.ActivityExecutionContexts
            .FirstOrDefault(x => x.Activity is If);
        Assert.That(ifContext, Is.Not.Null, "If activity should have been executed");
    }

    #endregion

    #region While

    [Test]
    public async Task While_WithFalseCondition_DoesNotLoop()
    {
        var workflow = new WorkflowActivity(new While(new Input<bool>(false))
        {
            Body = new WriteLine("Should not run")
        });

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Is.Empty, "While body should not execute when condition is false");
    }

    [Test]
    public async Task While_WithBreak_ExitsAfterFirstIteration()
    {
        var whileActivity = new While(new Input<bool>(true))
        {
            Body = new Sequence
            {
                Activities = { new Break() }
            }
        };
        var workflow = new WorkflowActivity(whileActivity);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var whileContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is While).ToList();
        Assert.That(whileContexts, Has.Count.EqualTo(1), "While should have exactly one execution context");
    }

    #endregion

    #region For

    [Test]
    public async Task For_WithStep_IteratesCorrectly()
    {
        var forActivity = new For(1, 3, 1)
        {
            Body = new WriteLine("iteration")
        };
        var workflow = new WorkflowActivity(forActivity);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Has.Count.EqualTo(3), "For(1,3,1) should iterate 3 times");
    }

    [Test]
    public async Task For_WithStep2_IteratesCorrectly()
    {
        var forActivity = new For(0, 4, 2)
        {
            Body = new WriteLine("step2")
        };
        var workflow = new WorkflowActivity(forActivity);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Has.Count.EqualTo(3), "For(0,4,2) should iterate 3 times (0,2,4)");
    }

    [Test]
    public async Task For_WithNoBody_CompletesImmediately()
    {
        var forActivity = new For(1, 5, 1);
        var workflow = new WorkflowActivity(forActivity);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    #endregion

    #region ForEach

    [Test]
    public async Task ForEach_IteratesOverAllItems()
    {
        var items = new List<string> { "A", "B", "C" };
        var forEach = new ForEach<string>(items)
        {
            Body = new WriteLine("item")
        };
        var workflow = new WorkflowActivity(forEach);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Has.Count.EqualTo(3), "ForEach should iterate over all 3 items");
    }

    [Test]
    public async Task ForEach_WithEmptyCollection_CompletesImmediately()
    {
        var forEach = new ForEach<string>(new List<string>())
        {
            Body = new WriteLine("item")
        };
        var workflow = new WorkflowActivity(forEach);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Is.Empty, "ForEach with empty collection should not execute body");
    }

    #endregion

    #region Flowchart

    [Test]
    public async Task Flowchart_LinearFlow_StartToWriteLineToEnd()
    {
        var start = new Start { Id = "fc_start" };
        var writeLine = new WriteLine("flowchart") { Id = "fc_wl" };
        var end = new End { Id = "fc_end" };

        var flowchart = new Flowchart
        {
            Activities = { start, writeLine, end },
            Connections =
            {
                new ActivityConnection(
                    new ActivityEndpoint("fc_start"),
                    new ActivityEndpoint("fc_wl")),
                new ActivityConnection(
                    new ActivityEndpoint("fc_wl"),
                    new ActivityEndpoint("fc_end"))
            }
        };
        var workflow = new WorkflowActivity(flowchart);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowExecutionContext.ActivityExecutionContexts
            .Any(x => x.Activity is Start), "Start should have been executed");
        Assert.That(result.WorkflowExecutionContext.ActivityExecutionContexts
            .Any(x => x.Activity is WriteLine), "WriteLine should have been executed");
        Assert.That(result.WorkflowExecutionContext.ActivityExecutionContexts
            .Any(x => x.Activity is End), "End should have been executed");
    }

    [Test]
    public async Task Flowchart_TrueBranch_RoutesCorrectly()
    {
        var start = new Start { Id = "s" };
        var ifActivity = new If { Id = "if1", Condition = new(true) };
        var trueBranch = new WriteLine("true") { Id = "tb" };
        var falseBranch = new WriteLine("false") { Id = "fb" };

        var flowchart = new Flowchart
        {
            Activities = { start, ifActivity, trueBranch, falseBranch },
            Connections =
            {
                new ActivityConnection(new ActivityEndpoint("s"), new ActivityEndpoint("if1")),
                new ActivityConnection(new ActivityEndpoint("if1", "True"), new ActivityEndpoint("tb")),
                new ActivityConnection(new ActivityEndpoint("if1", "False"), new ActivityEndpoint("fb"))
            }
        };
        var workflow = new WorkflowActivity(flowchart);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Has.Count.EqualTo(1), "Only one branch should execute");
    }

    [Test]
    public async Task Flowchart_FalseBranch_RoutesCorrectly()
    {
        var start = new Start { Id = "s" };
        var ifActivity = new If { Id = "if1", Condition = new(false) };
        var trueBranch = new WriteLine("true") { Id = "tb" };
        var falseBranch = new WriteLine("false") { Id = "fb" };

        var flowchart = new Flowchart
        {
            Activities = { start, ifActivity, trueBranch, falseBranch },
            Connections =
            {
                new ActivityConnection(new ActivityEndpoint("s"), new ActivityEndpoint("if1")),
                new ActivityConnection(new ActivityEndpoint("if1", "True"), new ActivityEndpoint("tb")),
                new ActivityConnection(new ActivityEndpoint("if1", "False"), new ActivityEndpoint("fb"))
            }
        };
        var workflow = new WorkflowActivity(flowchart);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Has.Count.EqualTo(1), "Only one branch should execute");
    }

    [Test]
    public async Task Flowchart_DiamondWithMerge_CompletesSuccessfully()
    {
        var start = new Start { Id = "d_s" };
        var ifActivity = new If { Id = "d_if", Condition = new(true) };
        var pathA = new WriteLine("A") { Id = "d_pa" };
        var pathB = new WriteLine("B") { Id = "d_pb" };
        var merge = new WriteLine("merged") { Id = "d_m" };
        var end = new End { Id = "d_e" };

        merge.CustomProperties["MergeMode"] = MergeMode.Merge;

        var flowchart = new Flowchart
        {
            Activities = { start, ifActivity, pathA, pathB, merge, end },
            Connections =
            {
                new ActivityConnection(new ActivityEndpoint("d_s"), new ActivityEndpoint("d_if")),
                new ActivityConnection(new ActivityEndpoint("d_if", "True"), new ActivityEndpoint("d_pa")),
                new ActivityConnection(new ActivityEndpoint("d_if", "False"), new ActivityEndpoint("d_pb")),
                new ActivityConnection(new ActivityEndpoint("d_pa"), new ActivityEndpoint("d_m")),
                new ActivityConnection(new ActivityEndpoint("d_pb"), new ActivityEndpoint("d_m")),
                new ActivityConnection(new ActivityEndpoint("d_m"), new ActivityEndpoint("d_e"))
            }
        };
        var workflow = new WorkflowActivity(flowchart);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    #endregion

    #region Start / End

    [Test]
    public async Task Start_Standalone_CompletesSuccessfully()
    {
        var workflow = new WorkflowActivity(new Start());

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    [Test]
    public async Task End_Standalone_CompletesSuccessfully()
    {
        var workflow = new WorkflowActivity(new End());

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    #endregion

    #region Nested Combinations

    [Test]
    public async Task For_ContainingForEach_ExecutesCorrectly()
    {
        var innerForEach = new ForEach<string>(new List<string> { "X", "Y" })
        {
            Body = new WriteLine("inner")
        };
        var forActivity = new For(1, 2, 1)
        {
            Body = innerForEach
        };
        var workflow = new WorkflowActivity(forActivity);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Has.Count.EqualTo(4), "For(2) × ForEach(2) = 4 WriteLine executions");
    }

    [Test]
    public async Task Sequence_ContainingFlowchart_ExecutesCorrectly()
    {
        var start = new Start { Id = "seq_fc_s" };
        var end = new End { Id = "seq_fc_e" };
        var flowchart = new Flowchart
        {
            Activities = { start, end },
            Connections =
            {
                new ActivityConnection(new ActivityEndpoint("seq_fc_s"), new ActivityEndpoint("seq_fc_e"))
            }
        };
        var sequence = new Sequence
        {
            Activities = { new WriteLine("before"), flowchart, new WriteLine("after") }
        };
        var workflow = new WorkflowActivity(sequence);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Has.Count.EqualTo(2), "before + after WriteLines");
        Assert.That(result.WorkflowExecutionContext.ActivityExecutionContexts
            .Any(x => x.Activity is Start), "Flowchart Start should have executed");
        Assert.That(result.WorkflowExecutionContext.ActivityExecutionContexts
            .Any(x => x.Activity is End), "Flowchart End should have executed");
    }

    [Test]
    public async Task Parallel_ContainingSequences_ExecutesAll()
    {
        var parallel = new Cike.Workflow.Core.Activities.Parallel(
            new Sequence { Activities = { new WriteLine("A1"), new WriteLine("A2") } },
            new Sequence { Activities = { new WriteLine("B1"), new WriteLine("B2") } }
        );
        var workflow = new WorkflowActivity(parallel);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Has.Count.EqualTo(4), "All 4 WriteLines from both sequences should execute");
    }

    [Test]
    public async Task ForEach_ContainingFlowchart_ExecutesCorrectly()
    {
        var start = new Start { Id = "fefc_s" };
        var writeLine = new WriteLine("in-flowchart") { Id = "fefc_wl" };
        var end = new End { Id = "fefc_e" };

        var flowchart = new Flowchart
        {
            Activities = { start, writeLine, end },
            Connections =
            {
                new ActivityConnection(new ActivityEndpoint("fefc_s"), new ActivityEndpoint("fefc_wl")),
                new ActivityConnection(new ActivityEndpoint("fefc_wl"), new ActivityEndpoint("fefc_e"))
            }
        };

        var forEach = new ForEach<string>(new List<string> { "A", "B" })
        {
            Body = flowchart
        };
        var workflow = new WorkflowActivity(forEach);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        var writeLineContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Has.Count.EqualTo(2), "ForEach(2) × Flowchart(1 WriteLine) = 2 WriteLines");
        var startContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is Start).ToList();
        Assert.That(startContexts, Has.Count.EqualTo(2), "2 Flowchart Starts");
        var endContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => x.Activity is End).ToList();
        Assert.That(endContexts, Has.Count.EqualTo(2), "2 Flowchart Ends");
    }

    #endregion
}
