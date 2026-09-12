namespace Cike.Workflow.Application.WorkflowDefinitions.Validators;

public class RollbackWorkflowDefinitionDtoValidator : AbstractValidator<RollbackWorkflowDefinitionDto>
{
    public RollbackWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.DefinitionId)
            .NotEmpty().WithMessage("工作流编号不能为空。");

        RuleFor(x => x.DefinitionVersionId)
            .GreaterThan(0).WithMessage("回滚目标版本不能为空。");
    }
}
