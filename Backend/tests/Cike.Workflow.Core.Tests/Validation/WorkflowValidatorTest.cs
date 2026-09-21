using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.FlowchartActivity;
using Cike.Workflow.Core.Validation;
using Cike.Workflow.Core.Validation.Internals;

namespace Cike.Workflow.Core.Tests.Validation;

[TestFixture]
public class WorkflowValidatorTest
{
    private readonly WorkflowValidator _validator = new();

    [Test]
    public void Validate_RootNotFlowchart_ReturnsSingleFlowchartError()
    {
        var context = new WorkflowValidationContext(new Sequence(), []);

        var errors = _validator.Validate(context);

        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0].Message, Does.Contain("画布根节点必须是流程图"));
    }

    [Test]
    public void Validate_VariableNameEmpty_ReturnsError()
    {
        var variables = new List<WorkflowVariableDefinition> { new("1", "", "Text", false) };
        var context = new WorkflowValidationContext(CreateValidCanvas(), variables);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("名称不能为空"));
    }

    [Test]
    public void Validate_VariableTypeNameEmpty_ReturnsError()
    {
        var variables = new List<WorkflowVariableDefinition> { new("1", "Amount", "", false) };
        var context = new WorkflowValidationContext(CreateValidCanvas(), variables);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("类型不能为空"));
    }

    [Test]
    public void Validate_DuplicateVariableNames_ReturnsError()
    {
        var variables = new List<WorkflowVariableDefinition>
        {
            new("1", "Amount", "Text", false),
            new("2", "Amount", "Text", false),
        };
        var context = new WorkflowValidationContext(CreateValidCanvas(), variables);

        var errors = _validator.Validate(context);

        Assert.That(errors.Select(x => x.Message), Has.Some.Contains("重复"));
    }

    [Test]
    public void Validate_ValidCanvas_ReturnsNoErrors()
    {
        var variables = new List<WorkflowVariableDefinition> { new("1", "Amount", "Text", false) };
        var context = new WorkflowValidationContext(CreateValidCanvas(), variables);

        var errors = _validator.Validate(context);

        Assert.That(errors, Is.Empty);
    }

    internal static Flowchart CreateValidCanvas()
    {
        var flowchart = new Flowchart { Id = "root" };
        var start = new Start { Id = "start" };
        flowchart.Activities.Add(start);
        flowchart.Start = start;
        return flowchart;
    }
}
