namespace Cike.Workflow.Application.WorkflowDefinitions.Validators;

public class PublishWorkflowDefinitionDtoValidator : AbstractValidator<PublishWorkflowDefinitionDto>
{
    public PublishWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.Root)
            .NotNull().WithMessage("画布内容不能为空。");

        RuleFor(x => x.PublishedNote)
            .MaximumLength(512).WithMessage("版本说明不能超过512个字符。");
    }
}
