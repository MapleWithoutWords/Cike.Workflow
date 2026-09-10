namespace Cike.Workflow.Application.WorkflowDefinitions.Validators;

public class UpdateWorkflowDefinitionDtoValidator : AbstractValidator<UpdateWorkflowDefinitionDto>
{
    public UpdateWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("工作流名称不能为空。")
            .MaximumLength(128).WithMessage("工作流名称不能超过128个字符。")
            .Matches(@"^[一-龥a-zA-Z0-9_-]+$").WithMessage("工作流名称只支持中文、英文字母、数字、下划线和连字符。");

        RuleFor(x => x.Description)
            .MaximumLength(512).WithMessage("工作流描述不能超过512个字符。");
    }
}
