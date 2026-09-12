namespace Cike.Workflow.Application.WorkflowDefinitions.Validators;

public class RollbackWorkflowDefinitionDtoValidator : AbstractValidator<RollbackWorkflowDefinitionDto>
{
    public RollbackWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.TargetVersion)
            .GreaterThan(0).WithMessage("回滚目标版本必须大于0。");
    }
}
