using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Activities.FlowchartActivity;
using Cike.Workflow.Core.Activities.FlowchartActivity.Models;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.Runners.Models;

namespace Cike.Workflow.Core.Tests.Runners;

/// <summary>
/// 全流程端到端集成测试，组合使用所有活动类型构建复杂工作流，
/// 运行后验证最终状态。
/// </summary>
[TestFixture]
public class FullWorkflowIntegrationTest : BaseIntegrationTest
{
    /// <summary>
    /// 组合所有活动类型的超级工作流：
    ///
    /// Sequence (root)
    /// ├── WriteLine("Step 1: Initial")
    /// ├── For(1, 2, 1) → 2 iterations
    /// │   └── ForEach(["X", "Y"]) → 2 items each
    /// │       └── Flowchart #1
    /// │           ├── Start → If(true) → WriteLine("True Branch")
    /// │           └── End
    /// ├── Parallel
    /// │   ├── Sequence
    /// │   │   ├── WriteLine("Parallel-A")
    /// │   │   └── While(true) { Break }
    /// │   └── WriteLine("Parallel-B")
    /// └── Flowchart #2
    ///     ├── Start → If(false) → WriteLine("True") / WriteLine("False")
    ///     └── End
    ///
    /// 覆盖的活动类型: Sequence, For, ForEach, Flowchart, Start, End, If,
    ///                 Parallel, While, Break, WriteLine
    /// </summary>
    private static WorkflowActivity BuildMegaWorkflow()
    {
        // --- Flowchart #1: Start → If(true) → TrueBranch / FalseBranch → End ---
        var fc1Start = new Start { Id = "fc1_s" };
        var fc1If = new If { Id = "fc1_if", Condition = new(true) };
        var fc1True = new WriteLine("True Branch") { Id = "fc1_t" };
        var fc1False = new WriteLine("False Branch") { Id = "fc1_f" };
        var fc1End = new End { Id = "fc1_e" };

        var flowchart1 = new Flowchart
        {
            Activities = { fc1Start, fc1If, fc1True, fc1False, fc1End },
            Connections =
            {
                new ActivityConnection(new ActivityEndpoint("fc1_s"), new ActivityEndpoint("fc1_if")),
                new ActivityConnection(new ActivityEndpoint("fc1_if", "True"), new ActivityEndpoint("fc1_t")),
                new ActivityConnection(new ActivityEndpoint("fc1_if", "False"), new ActivityEndpoint("fc1_f")),
                new ActivityConnection(new ActivityEndpoint("fc1_t"), new ActivityEndpoint("fc1_e")),
                new ActivityConnection(new ActivityEndpoint("fc1_f"), new ActivityEndpoint("fc1_e"))
            }
        };

        // --- For(1,2,1) → ForEach(["X","Y"]) → Flowchart #1 ---
        var forEach = new ForEach<string>(new List<string> { "X", "Y" })
        {
            Body = flowchart1
        };
        var forActivity = new For(1, 2, 1)
        {
            Body = forEach
        };

        // --- Parallel: Sequence(WriteLine, While+Break) | WriteLine ---
        var parallel = new Cike.Workflow.Core.Activities.Parallel(
            new Sequence
            {
                Activities =
                {
                    new WriteLine("Parallel-A"),
                    new While(new Input<bool>(true))
                    {
                        Body = new Sequence { Activities = { new Break() } }
                    }
                }
            },
            new WriteLine("Parallel-B")
        );

        // --- Flowchart #2: Start → If(false) → TrueBranch / FalseBranch → End ---
        var fc2Start = new Start { Id = "fc2_s" };
        var fc2If = new If { Id = "fc2_if", Condition = new(false) };
        var fc2True = new WriteLine("FC2-True") { Id = "fc2_t" };
        var fc2False = new WriteLine("FC2-False") { Id = "fc2_f" };
        var fc2End = new End { Id = "fc2_e" };

        var flowchart2 = new Flowchart
        {
            Activities = { fc2Start, fc2If, fc2True, fc2False, fc2End },
            Connections =
            {
                new ActivityConnection(new ActivityEndpoint("fc2_s"), new ActivityEndpoint("fc2_if")),
                new ActivityConnection(new ActivityEndpoint("fc2_if", "True"), new ActivityEndpoint("fc2_t")),
                new ActivityConnection(new ActivityEndpoint("fc2_if", "False"), new ActivityEndpoint("fc2_f")),
                new ActivityConnection(new ActivityEndpoint("fc2_t"), new ActivityEndpoint("fc2_e")),
                new ActivityConnection(new ActivityEndpoint("fc2_f"), new ActivityEndpoint("fc2_e"))
            }
        };

        // --- Root Sequence ---
        var rootSequence = new Sequence
        {
            Activities =
            {
                new WriteLine("Step 1: Initial"),
                forActivity,
                parallel,
                flowchart2
            }
        };

        return new WorkflowActivity(rootSequence);
    }

    [Test]
    public async Task MegaWorkflow_AllActivityTypes_CompletesSuccessfully()
    {
        var workflow = BuildMegaWorkflow();

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    [Test]
    public async Task MegaWorkflow_VerifyAllActivityTypesExecuted()
    {
        var workflow = BuildMegaWorkflow();

        var result = await runner.RunAsync(workflow);

        var contexts = result.WorkflowExecutionContext.ActivityExecutionContexts;
        var activityTypes = contexts.Select(x => x.Activity.GetType().Name).Distinct().ToList();

        Assert.That(activityTypes, Does.Contain("Sequence"), "Sequence should have been executed");
        Assert.That(activityTypes, Does.Contain("For"), "For should have been executed");
        Assert.That(activityTypes, Does.Contain("ForEach`1"), "ForEach should have been executed");
        Assert.That(activityTypes, Does.Contain("Flowchart"), "Flowchart should have been executed");
        Assert.That(activityTypes, Does.Contain("Start"), "Start should have been executed");
        Assert.That(activityTypes, Does.Contain("End"), "End should have been executed");
        Assert.That(activityTypes, Does.Contain("If"), "If should have been executed");
        Assert.That(activityTypes, Does.Contain("Parallel"), "Parallel should have been executed");
        Assert.That(activityTypes, Does.Contain("While"), "While should have been executed");
        Assert.That(activityTypes, Does.Contain("Break"), "Break should have been executed");
        Assert.That(activityTypes, Does.Contain("WriteLine"), "WriteLine should have been executed");
    }

    [Test]
    public async Task MegaWorkflow_VerifyExecutionCounts()
    {
        var workflow = BuildMegaWorkflow();

        var result = await runner.RunAsync(workflow);

        var contexts = result.WorkflowExecutionContext.ActivityExecutionContexts;

        var forContexts = contexts.Where(x => x.Activity is For).ToList();
        Assert.That(forContexts, Has.Count.EqualTo(1), "For runs once");

        var forEachContexts = contexts.Where(x => x.Activity.GetType().Name.StartsWith("ForEach")).ToList();
        Assert.That(forEachContexts, Has.Count.EqualTo(2), "ForEach runs 2 times (once per For iteration)");

        var flowchartContexts = contexts.Where(x => x.Activity is Flowchart).ToList();
        Assert.That(flowchartContexts, Has.Count.EqualTo(5), "Flowchart #1 runs 4 times + Flowchart #2 runs 1 time = 5");

        var startContexts = contexts.Where(x => x.Activity is Start).ToList();
        Assert.That(startContexts, Has.Count.EqualTo(4 + 1), "4 FC1 Starts + 1 FC2 Start = 5");

        var endContexts = contexts.Where(x => x.Activity is End).ToList();
        Assert.That(endContexts, Has.Count.EqualTo(5), "4 FC1 Ends + 1 FC2 End = 5");

        var ifContexts = contexts.Where(x => x.Activity is If).ToList();
        Assert.That(ifContexts, Has.Count.EqualTo(5), "4 FC1 Ifs + 1 FC2 If = 5");

        var whileContexts = contexts.Where(x => x.Activity is While).ToList();
        Assert.That(whileContexts, Has.Count.EqualTo(1), "While runs once in Parallel");

        var breakContexts = contexts.Where(x => x.Activity is Break).ToList();
        Assert.That(breakContexts, Has.Count.EqualTo(1), "Break runs once");

        // WriteLine: "Step 1"=1, "True Branch"=4, "Parallel-A"=1, "Parallel-B"=1, "FC2-False"=1 = 8
        var writeLineContexts = contexts.Where(x => x.Activity is WriteLine).ToList();
        Assert.That(writeLineContexts, Has.Count.EqualTo(8), "Total WriteLines across all paths");
    }

    [Test]
    public async Task MegaWorkflow_WorkflowStateIsFullyPopulated()
    {
        var workflow = BuildMegaWorkflow();

        var result = await runner.RunAsync(workflow);
        var state = result.WorkflowState;

        Assert.That(state.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(state.FinishedAt, Is.Not.Null);
        Assert.That(state.CreatedAt, Is.LessThanOrEqualTo(state.FinishedAt.Value));
        Assert.That(state.DefinitionId, Is.Not.Null.And.Not.Empty);
        Assert.That(state.Id, Is.GreaterThan(0));
    }

    [Test]
    public async Task MegaWorkflow_JournalContainsAllContexts()
    {
        var workflow = BuildMegaWorkflow();

        var result = await runner.RunAsync(workflow);

        Assert.That(result.Journal, Is.Not.Null);
        Assert.That(result.Journal.ActivityExecutionContexts, Is.Not.Empty);
        Assert.That(result.Journal.ActivityExecutionContexts.Count, Is.GreaterThan(20),
            "Complex workflow should have many activity execution contexts");
    }

    [Test]
    public async Task MegaWorkflow_WithOptions_PreservesOptions()
    {
        var workflow = BuildMegaWorkflow();
        var options = new RunWorkflowOptions
        {
            CorrelationId = "mega-corr-id",
            WorkflowInstanceId = 999L,
            Input = new Dictionary<string, object> { ["testKey"] = "testValue" }
        };

        var result = await runner.RunAsync(workflow, options);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowState.CorrelationId, Is.EqualTo("mega-corr-id"));
        Assert.That(result.WorkflowState.Id, Is.EqualTo(999L));
        Assert.That(result.WorkflowExecutionContext.Input, Contains.Key("testKey"));
    }

    [Test]
    public async Task MegaWorkflow_MultipleSequentialRuns_ProduceConsistentResults()
    {
        var results = new List<RunWorkflowResult>();
        for (var i = 0; i < 3; i++)
        {
            var w = BuildMegaWorkflow();
            results.Add(await runner.RunAsync(w));
        }

        Assert.That(results.All(r => r.WorkflowState.Status == WorkflowStatus.Finished),
            "All runs should complete successfully");

        var contextCounts = results.Select(r => r.WorkflowExecutionContext.ActivityExecutionContexts.Count).Distinct().ToList();
        Assert.That(contextCounts, Has.Count.EqualTo(1),
            "All runs should produce the same number of activity execution contexts");
    }

    [Test]
    public async Task MegaWorkflow_VerifyWorkflowStatusTransitions()
    {
        var workflow = BuildMegaWorkflow();

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowExecutionContext.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowExecutionContext.FinishedAt, Is.Not.Null);

        var incompleteContexts = result.WorkflowExecutionContext.ActivityExecutionContexts
            .Where(x => !x.IsCompleted)
            .ToList();
        Assert.That(incompleteContexts, Is.Empty,
            "All activity execution contexts should be completed after workflow finishes");
    }
}
