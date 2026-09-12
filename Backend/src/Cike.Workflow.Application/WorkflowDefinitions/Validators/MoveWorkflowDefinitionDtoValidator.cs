namespace Cike.Workflow.Application.WorkflowDefinitions.Validators;

public class MoveWorkflowDefinitionDtoValidator : AbstractValidator<MoveWorkflowDefinitionDto>
{
    public MoveWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.FolderId)
            .GreaterThanOrEqualTo(0).WithMessage("目标目录不能为负数。");
    }
}
