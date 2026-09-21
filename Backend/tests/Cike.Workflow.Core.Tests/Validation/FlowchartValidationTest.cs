using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Activities.FlowchartActivity;
using Cike.Workflow.Core.Activities.FlowchartActivity.Models;
using Cike.Workflow.Core.Validation;
using Cike.Workflow.Core.Validation.Internals;

namespace Cike.Workflow.Core.Tests.Validation;

[TestFixture]
public class FlowchartValidationTest
{
    private readonly WorkflowValidator _validator = new();

    [Test]
    public void Validate_StartMissing_ReturnsLocatableError()
    {
        var flowchart = new Flowchart { Id = "root" };
        var write = new WriteLineStub { Id = "write" };
        flowchart.Activities.Add(write);
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("未找到开始节点"));
    }

    [Test]
    public void Validate_StartActivityInCanvas_PassesStartCheck()
    {
        var flowchart = new Flowchart { Id = "root" };
        var start = new Start { Id = "start" };
        flowchart.Activities.Add(start);
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.Message), Has.None.Contains("未找到开始节点"));
    }

    [Test]
    public void Validate_CanStartWorkflowActivity_PassesStartCheck()
    {
        var flowchart = new Flowchart { Id = "root" };
        var starter = new WriteLineStub { Id = "starter" };
        ((IActivity)starter).SetCanStartWorkflow(true);
        flowchart.Activities.Add(starter);
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.Message), Has.None.Contains("未找到开始节点"));
    }

    [Test]
    public void Validate_ConnectionReferencesMissingActivity_ReturnsLocatableError()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var write = new WriteLineStub { Id = "write" };
        flowchart.Activities.Add(write);
        flowchart.Connections.Add(new ActivityConnection(new ActivityEndpoint("start"), new ActivityEndpoint("write")));
        flowchart.Connections.Add(new ActivityConnection(new ActivityEndpoint("write"), new ActivityEndpoint("ghost")));
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.ActivityId), Has.Some.EqualTo("ghost"));
        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("连线引用了不存在"));

        // 悬空节点无从取 Name/NodeId，报错定位字段允许为空
        var located = errors.Single(x => x.ActivityId == "ghost" && x.Message.Contains("连线引用了不存在"));
        Assert.That(located.Name, Is.Null);
        Assert.That(located.NodeId, Is.Null);
    }

    [Test]
    public void Validate_OrphanNode_ReturnsLocatableError()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var orphan = new WriteLineStub { Id = "orphan", Name = "孤儿节点", NodeId = "node_orphan" };
        flowchart.Activities.Add(orphan);
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.ActivityId), Has.Some.EqualTo("orphan"));
        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("孤立节点"));

        var located = errors.Single(x => x.ActivityId == "orphan" && x.Message.Contains("孤立节点"));
        Assert.That(located.Name, Is.EqualTo("孤儿节点"));
        Assert.That(located.NodeId, Is.EqualTo("node_orphan"));
    }

    [Test]
    public void Validate_StartActivityAlone_IsExemptFromOrphanCheck()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_CanStartWorkflowActivityUnconnected_IsExemptFromOrphanCheck()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var starter = new WriteLineStub { Id = "starter" };
        ((IActivity)starter).SetCanStartWorkflow(true);
        flowchart.Activities.Add(starter);
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.Message), Has.None.Contains("孤立节点"));
    }

    [Test]
    public void Validate_ActivityReferencedAsStart_IsExemptFromOrphanCheck()
    {
        var flowchart = new Flowchart { Id = "root" };
        var entry = new WriteLineStub { Id = "entry" };
        flowchart.Activities.Add(entry);
        flowchart.Start = entry;
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.Message), Has.None.Contains("孤立节点"));
    }

    private sealed class WriteLineStub : Activity
    {
    }
}
