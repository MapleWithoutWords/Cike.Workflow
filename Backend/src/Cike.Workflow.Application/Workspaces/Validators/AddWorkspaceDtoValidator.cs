namespace Cike.Workflow.Application.Workspaces.Validators;

public class AddWorkspaceDtoValidator : AbstractValidator<AddWorkspaceDto>
{
    public AddWorkspaceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("工作空间名称不能为空。")
            .MaximumLength(128).WithMessage("工作空间名称不能超过128个字符。")
            .Matches(@"^[一-龥a-zA-Z0-9_-]+$").WithMessage("工作空间名称只支持中文、英文字母、数字、下划线和连字符。");

        When(x => !string.IsNullOrEmpty(x.Code), () =>
        {
            RuleFor(x => x.Code)
                .MaximumLength(128).WithMessage("工作空间编号不能超过128个字符。")
                .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("工作空间编号只支持英文字母、数字、下划线和连字符。");
        });

        RuleFor(x => x.Description)
            .MaximumLength(512).WithMessage("工作空间描述不能超过512个字符。");
    }
}
