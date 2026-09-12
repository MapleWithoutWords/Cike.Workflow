namespace Cike.Workflow.Application.WorkflowDefinitions.Validators;

public class SaveWorkflowDefinitionDtoValidator : AbstractValidator<SaveWorkflowDefinitionDto>
{
    public SaveWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.Body)
            .NotNull().WithMessage("画布内容不能为空。");
    }
}
