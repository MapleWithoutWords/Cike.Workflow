namespace Cike.Workflow.Application.WorkflowDefinitions.Validators;

public class AddWorkflowDefinitionDtoValidator : AbstractValidator<AddWorkflowDefinitionDto>
{
    public AddWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEqual(0).WithMessage("所属工作空间不能为空。");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("工作流名称不能为空。")
            .MaximumLength(128).WithMessage("工作流名称不能超过128个字符。")
            .Matches(@"^[一-龥a-zA-Z0-9_-]+$").WithMessage("工作流名称只支持中文、英文字母、数字、下划线和连字符。");

        When(x => !string.IsNullOrEmpty(x.DefinitionId), () =>
        {
            RuleFor(x => x.DefinitionId)
                .MaximumLength(128).WithMessage("工作流编号不能超过128个字符。")
                .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("工作流编号只支持英文字母、数字、下划线和连字符。");
        });

        RuleFor(x => x.Description)
            .MaximumLength(512).WithMessage("工作流描述不能超过512个字符。");
    }
}
