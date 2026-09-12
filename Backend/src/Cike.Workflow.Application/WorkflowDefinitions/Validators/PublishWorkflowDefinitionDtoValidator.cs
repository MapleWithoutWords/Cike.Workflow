namespace Cike.Workflow.Application.WorkflowDefinitions.Validators;

public class PublishWorkflowDefinitionDtoValidator : AbstractValidator<PublishWorkflowDefinitionDto>
{
    public PublishWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.PublishedNote)
            .MaximumLength(512).WithMessage("版本说明不能超过512个字符。");
    }
}
