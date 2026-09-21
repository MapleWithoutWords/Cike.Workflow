using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Activities.FlowchartActivity;
using Cike.Workflow.Core.Activities.FlowchartActivity.Models;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.Validation;
using Cike.Workflow.Core.Validation.Internals;
using InputAttribute = Cike.Workflow.Core.Attributes.InputAttribute;

namespace Cike.Workflow.Core.Tests.Validation;

[TestFixture]
public class ActivityValidationRecursionTest
{
    private readonly WorkflowValidator _validator = new();

    [Test]
    public void Validate_SequenceChildMissingRequiredInput_ReturnsLocatableError()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var sequence = new Sequence { Id = "sequence" };
        var bad = new InputHolderActivity { Id = "bad" };
        sequence.Activities.Add(bad);
        flowchart.Activities.Add(sequence);
        flowchart.Connections.Add(new ActivityConnection(new ActivityEndpoint("start"), new ActivityEndpoint("sequence")));
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.ActivityId), Has.Some.EqualTo("bad"));
        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("必填属性"));
    }

    [Test]
    public void Validate_ForBodyMissingRequiredInput_ReturnsLocatableError()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var forLoop = new For { Id = "for" };
        forLoop.Body = new InputHolderActivity { Id = "bad" };
        flowchart.Activities.Add(forLoop);
        flowchart.Connections.Add(new ActivityConnection(new ActivityEndpoint("start"), new ActivityEndpoint("for")));
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.ActivityId), Has.Some.EqualTo("bad"));
        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("必填属性"));
    }

    [Test]
    public void Validate_WhileBodyMissingRequiredInput_ReturnsLocatableError()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var whileLoop = new While { Id = "while" };
        whileLoop.Body = new InputHolderActivity { Id = "bad" };
        flowchart.Activities.Add(whileLoop);
        flowchart.Connections.Add(new ActivityConnection(new ActivityEndpoint("start"), new ActivityEndpoint("while")));
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.ActivityId), Has.Some.EqualTo("bad"));
        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("必填属性"));
    }

    [Test]
    public void Validate_ForEachBodyMissingRequiredInput_ReturnsLocatableError()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var forEach = new ForEach<int> { Id = "foreach" };
        forEach.Body = new InputHolderActivity { Id = "bad" };
        flowchart.Activities.Add(forEach);
        flowchart.Connections.Add(new ActivityConnection(new ActivityEndpoint("start"), new ActivityEndpoint("foreach")));
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.ActivityId), Has.Some.EqualTo("bad"));
        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("必填属性"));
    }

    [Test]
    public void Validate_CompositeRootSubtreeMissingRequiredInput_ReturnsLocatableError()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var composite = new WorkflowActivity(new Sequence { Id = "composite-root" });
        ((Sequence)composite.Root).Activities.Add(new InputHolderActivity { Id = "bad" });
        flowchart.Activities.Add(composite);
        flowchart.Connections.Add(new ActivityConnection(new ActivityEndpoint("start"), new ActivityEndpoint(composite.Id)));
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.ActivityId), Has.Some.EqualTo("bad"));
        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("必填属性"));
    }

    [Test]
    public void Validate_CustomActivityOverrideWithBaseCall_RunsDefaultAndCustomRules()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var guarded = new GuardedActivity { Id = "guarded" };
        flowchart.Activities.Add(guarded);
        flowchart.Connections.Add(new ActivityConnection(new ActivityEndpoint("start"), new ActivityEndpoint("guarded")));
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Where(x => x.ActivityId == "guarded").Select(x => x.Message),
            Has.Some.Contains("必填属性"));
        Assert.That(errors.Where(x => x.ActivityId == "guarded").Select(x => x.Message),
            Has.Some.Contains("守护配置"));
    }

    [Test]
    public void Validate_CustomActivityOverrideWithoutBaseCall_SkipsDefaultRule()
    {
        var flowchart = WorkflowValidatorTest.CreateValidCanvas();
        var silent = new SilentActivity { Id = "silent" };
        flowchart.Activities.Add(silent);
        flowchart.Connections.Add(new ActivityConnection(new ActivityEndpoint("start"), new ActivityEndpoint("silent")));
        var context = new WorkflowValidationContext(flowchart, []);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.ActivityId), Has.None.EqualTo("silent"));
    }

    /// <summary>挂一个可空的 Input 属性，用于构造"缺必填"的子活动。</summary>
    private sealed class InputHolderActivity : Activity
    {
        [Input(Description = "required payload")]
        public Input<string>? Payload { get; set; }
    }

    /// <summary>调用 base 的扩展点示范活动：默认必填规则 + 自定义规则同时生效。</summary>
    private sealed class GuardedActivity : Activity
    {
        [Input(Description = "required payload")]
        public Input<string>? Payload { get; set; }

        protected override void Validate(WorkflowValidationContext context)
        {
            base.Validate(context);

            if (!Metadata.ContainsKey("guard"))
                context.Errors.Add(new(Id, $"节点 [{Id}] 缺少守护配置。"));
        }
    }

    /// <summary>不调用 base 的扩展点示范活动：默认必填规则被显式让位。</summary>
    private sealed class SilentActivity : Activity
    {
        [Input(Description = "required payload")]
        public Input<string>? Payload { get; set; }

        protected override void Validate(WorkflowValidationContext context)
        {
        }
    }
}
