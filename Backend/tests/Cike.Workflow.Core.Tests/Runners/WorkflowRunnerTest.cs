using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Exceptions;
using Cike.Workflow.Core.Runners.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Tests.Runners;

[TestFixture]
public class WorkflowRunnerTest : BaseIntegrationTest
{
    [Test]
    public async Task RunAsync_WithSingleWriteLine_CompletesSuccessfully()
    {
        var workflow = new WorkflowActivity(new WriteLine("Hello"));

        var result = await runner.RunAsync(workflow);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    [Test]
    public async Task RunAsync_WithSequence_ExecutesAllActivitiesInOrder()
    {
        var sequence = new Sequence
        {
            Activities =
            {
                new WriteLine("First"),
                new WriteLine("Second"),
                new WriteLine("Third")
            }
        };
        var workflow = new WorkflowActivity(sequence);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowExecutionContext.ActivityExecutionContexts.Count, Is.GreaterThanOrEqualTo(3));
    }

    [Test]
    public async Task RunAsync_WithEmptySequence_CompletesSuccessfully()
    {
        var sequence = new Sequence();
        var workflow = new WorkflowActivity(sequence);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    [Test]
    public async Task RunAsync_WithParallel_ExecutesAllChildren()
    {
        var parallel = new Cike.Workflow.Core.Activities.Parallel(
            new WriteLine("A"),
            new WriteLine("B"),
            new WriteLine("C")
        );
        var workflow = new WorkflowActivity(parallel);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    [Test]
    public async Task RunAsync_WithEmptyParallel_CompletesSuccessfully()
    {
        var parallel = new Cike.Workflow.Core.Activities.Parallel();
        var workflow = new WorkflowActivity(parallel);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    [Test]
    public async Task RunAsync_WithOptions_CorrelationIdIsPreserved()
    {
        var workflow = new WorkflowActivity(new WriteLine("test"));
        var options = new RunWorkflowOptions { CorrelationId = "corr-123" };

        var result = await runner.RunAsync(workflow, options);

        Assert.That(result.WorkflowState.CorrelationId, Is.EqualTo("corr-123"));
    }

    [Test]
    public async Task RunAsync_WithOptions_WorkflowInstanceIdIsPreserved()
    {
        var workflow = new WorkflowActivity(new WriteLine("test"));
        var options = new RunWorkflowOptions { WorkflowInstanceId = 42L };

        var result = await runner.RunAsync(workflow, options);

        Assert.That(result.WorkflowState.Id, Is.EqualTo(42L));
    }

    [Test]
    public async Task RunAsync_WithInput_InputIsAccessible()
    {
        var workflow = new WorkflowActivity(new WriteLine("test"));
        var options = new RunWorkflowOptions
        {
            Input = new Dictionary<string, object> { ["key1"] = "value1" }
        };

        var result = await runner.RunAsync(workflow, options);

        Assert.That(result.WorkflowExecutionContext.Input, Contains.Key("key1"));
    }

    [Test]
    public async Task RunAsync_WithFault_ThrowsFaultException()
    {
        var fault = Fault.Create("ERR001", "Test", "Business", "Test fault");
        var workflow = new WorkflowActivity(fault);

        Assert.ThrowsAsync<FaultException>(async () => await runner.RunAsync(workflow));
    }

    [Test]
    public async Task RunAsync_ReturnsJournalWithActivityContexts()
    {
        var workflow = new WorkflowActivity(new WriteLine("test"));

        var result = await runner.RunAsync(workflow);

        Assert.That(result.Journal, Is.Not.Null);
        Assert.That(result.Journal.ActivityExecutionContexts, Is.Not.Empty);
    }

    [Test]
    public async Task RunAsync_WithIActivity_ExecutesSuccessfully()
    {
        IActivity activity = new WriteLine("via IActivity");

        var result = await runner.RunAsync(activity);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    [Test]
    public async Task RunAsync_WithCancellationToken_CancelsGracefully()
    {
        using var cts = new CancellationTokenSource();
        var workflow = new WorkflowActivity(new WriteLine("runs before cancellation"));

        var result = await runner.RunAsync(workflow, cancellationToken: cts.Token);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
    }

    [Test]
    public async Task RunAsync_WorkflowStateIsExtractedCorrectly()
    {
        var workflow = new WorkflowActivity(new WriteLine("test"));

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState, Is.Not.Null);
        Assert.That(result.WorkflowState.DefinitionId, Is.Not.Null);
        Assert.That(result.WorkflowState.CreatedAt, Is.LessThanOrEqualTo(DateTime.Now));
    }

    [Test]
    public async Task RunAsync_NestedSequences_ExecuteCorrectly()
    {
        var innerSequence = new Sequence
        {
            Activities = { new WriteLine("inner1"), new WriteLine("inner2") }
        };
        var outerSequence = new Sequence
        {
            Activities = { new WriteLine("outer1"), innerSequence, new WriteLine("outer2") }
        };
        var workflow = new WorkflowActivity(outerSequence);

        var result = await runner.RunAsync(workflow);

        Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        Assert.That(result.WorkflowExecutionContext.ActivityExecutionContexts.Count, Is.GreaterThanOrEqualTo(4));
    }
}
