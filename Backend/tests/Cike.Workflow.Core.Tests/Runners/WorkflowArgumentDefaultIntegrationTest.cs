using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.Runners;
using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.Tests.Helpers;
using Cike.Workflow.Core.Variables;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Tests.Runners;

/// <summary>
/// 工作流输入/输出默认值赋值集成测试：经真实 IWorkflowRunner（本地事件总线 + 异常中间件）运行，
/// 只断言实例状态的可观察行为（Input / Output / Status / Incidents）。
/// </summary>
[TestFixture]
[Category("Integration")]
public class WorkflowArgumentDefaultIntegrationTest : BaseIntegrationTest
{
    private IWorkflowRunner realRunner = null!;

    [SetUp]
    public void SetUp() => realRunner = serviceProvider.GetRequiredService<IWorkflowRunner>();

    [Test]
    public async Task RunAsync_WithMissingInputAndLiteralDefault_MaterializesInputDefault()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Inputs.Add(new InputDefinition
        {
            Name = "Greeting",
            Type = "String",
            DefaultValue = TestExpressions.Literal("Hello")
        });

        var result = await realRunner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowState.Input, Does.ContainKey("Greeting"));
        Assert.That(result.WorkflowState.Input["Greeting"], Is.EqualTo("Hello"));
    }

    [Test]
    public async Task RunAsync_WithExplicitNullInput_DoesNotApplyDefault()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Inputs.Add(new InputDefinition
        {
            Name = "Greeting",
            Type = "String",
            DefaultValue = TestExpressions.Literal("Hello")
        });

        var result = await realRunner.RunAsync(workflow, new RunWorkflowOptions
        {
            Input = new Dictionary<string, object> { ["Greeting"] = null! }
        });

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowState.Input, Does.ContainKey("Greeting"));
        Assert.That(result.WorkflowState.Input["Greeting"], Is.Null);
    }

    [Test]
    public async Task RunAsync_WithCallerProvidedInput_KeepsCallerValueOverDefault()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Inputs.Add(new InputDefinition
        {
            Name = "Greeting",
            Type = "String",
            DefaultValue = TestExpressions.Literal("Hello")
        });

        var result = await realRunner.RunAsync(workflow, new RunWorkflowOptions
        {
            Input = new Dictionary<string, object> { ["Greeting"] = "Hi" }
        });

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowState.Input["Greeting"], Is.EqualTo("Hi"));
    }

    [Test]
    public async Task RunAsync_WithLiquidInputDefault_ConvertsToDeclaredType()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Inputs.Add(new InputDefinition
        {
            Name = "Count",
            Type = "Int32",
            DefaultValue = TestExpressions.Of("Liquid", "{{ 40 | plus: 2 }}")
        });

        var result = await realRunner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished),
            "INCIDENTS: " + System.Text.Json.JsonSerializer.Serialize(result.WorkflowState.Incidents));
        Assert.That(result.WorkflowState.Input["Count"], Is.EqualTo(42));
    }

    [Test]
    public async Task RunAsync_WithUnwrittenOutputAndLiteralDefault_MaterializesOutputDefault()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Outputs.Add(new OutputDefinition
        {
            Name = "Result",
            Type = "String",
            DefaultValue = TestExpressions.Literal("Done")
        });

        var result = await realRunner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowState.Output, Does.ContainKey("Result"));
        Assert.That(result.WorkflowState.Output["Result"], Is.EqualTo("Done"));
    }

    [Test]
    public async Task RunAsync_WithUnconfiguredInputDefault_LeavesInputMissing()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Inputs.Add(new InputDefinition
        {
            Name = "Greeting",
            Type = "String"
        });

        var result = await realRunner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowState.Input, Does.Not.ContainKey("Greeting"));
    }

    [Test]
    public async Task RunAsync_WithUnconfiguredOutputDefault_LeavesOutputMissing()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Outputs.Add(new OutputDefinition
        {
            Name = "Result",
            Type = "String"
        });

        var result = await realRunner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowState.Output, Does.Not.ContainKey("Result"));
    }

    [Test]
    public async Task RunAsync_WithOutputDefaultReferencingVariable_MaterializesFromFinalVariable()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Variables.Add(new Variable<int> { Name = "Base", Value = 40 });
        workflow.Outputs.Add(new OutputDefinition
        {
            Name = "Result",
            Type = "Int32",
            DefaultValue = TestExpressions.Of("Variable", "Base")
        });

        var result = await realRunner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished),
            "INCIDENTS: " + System.Text.Json.JsonSerializer.Serialize(result.WorkflowState.Incidents));
        Assert.That(result.WorkflowState.Output["Result"], Is.EqualTo(40));
    }

    [Test]
    public async Task RunAsync_WithOutputDefaultReferencingInput_MaterializesFromFinalInput()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Inputs.Add(new InputDefinition
        {
            Name = "Greeting",
            Type = "String",
            DefaultValue = TestExpressions.Literal("Hello")
        });
        workflow.Outputs.Add(new OutputDefinition
        {
            Name = "Result",
            Type = "String",
            DefaultValue = TestExpressions.Of("Input", "Greeting")
        });

        var result = await realRunner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished),
            "INCIDENTS: " + System.Text.Json.JsonSerializer.Serialize(result.WorkflowState.Incidents));
        Assert.That(result.WorkflowState.Output["Result"], Is.EqualTo("Hello"));
    }

    [Test]
    public async Task RunAsync_WithExplicitlyWrittenNullOutput_DoesNotApplyDefault()
    {
        var workflow = new WorkflowActivity(new WriteWorkflowOutput("Result", null));
        workflow.Outputs.Add(new OutputDefinition
        {
            Name = "Result",
            Type = "String",
            DefaultValue = TestExpressions.Literal("Done")
        });

        var result = await realRunner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowState.Output, Does.ContainKey("Result"));
        Assert.That(result.WorkflowState.Output["Result"], Is.Null);
    }

    [Test]
    public async Task RunAsync_WithFailingJavaScriptInputDefault_FaultsWorkflowWithIncident()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Inputs.Add(new InputDefinition
        {
            Name = "Broken",
            Type = "String",
            DefaultValue = TestExpressions.Of("JavaScript", "throw new Error('boom')")
        });

        var result = await realRunner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Faulted));
        Assert.That(result.WorkflowState.Incidents, Is.Not.Empty);
        Assert.That(result.WorkflowState.Incidents.Single().Message, Does.Contain("Broken"));
    }

    [Test]
    public async Task RunAsync_WithFailingJavaScriptOutputDefault_RecordsIncidentAndKeepsFinished()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));
        workflow.Outputs.Add(new OutputDefinition
        {
            Name = "Broken",
            Type = "String",
            DefaultValue = TestExpressions.Of("JavaScript", "throw new Error('boom')")
        });

        var result = await realRunner.RunAsync(workflow);

        // 终态转换在 handler 内已完成，输出默认值求值失败由异常中间件记录 incident，不改变状态。
        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowState.Incidents, Is.Not.Empty);
        Assert.That(result.WorkflowState.Incidents.Single().Message, Does.Contain("Broken"));
    }

    [Test]
    public async Task RunAsync_WhenResumedFromSuspendedState_KeepsMaterializedInputDefault()
    {
        var workflow = new WorkflowActivity(new SuspendingActivity());
        workflow.Inputs.Add(new InputDefinition
        {
            Name = "Token",
            Type = "String",
            DefaultValue = TestExpressions.Of("JavaScript", "Math.random().toString()")
        });

        var firstRun = await realRunner.RunAsync(workflow);

        Assert.That(firstRun.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Suspended),
            "INCIDENTS: " + System.Text.Json.JsonSerializer.Serialize(firstRun.WorkflowState.Incidents));
        var token = firstRun.WorkflowState.Input["Token"];

        // Simulate a resume with a fresh canvas instance + the first run's state snapshot
        // (production resumes by rebuilding the graph and applying the persisted state).
        var resumedWorkflow = new WorkflowActivity(new SuspendingActivity());
        resumedWorkflow.Inputs.Add(new InputDefinition
        {
            Name = "Token",
            Type = "String",
            DefaultValue = TestExpressions.Of("JavaScript", "Math.random().toString()")
        });

        var secondRun = await realRunner.RunAsync(resumedWorkflow, firstRun.WorkflowState);

        Assert.That(secondRun.WorkflowState.Input["Token"], Is.EqualTo(token),
            "恢复后输入默认值应保持首次运行物化的值，而不是重新求值");
    }
}
