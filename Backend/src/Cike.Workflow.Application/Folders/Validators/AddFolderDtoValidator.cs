namespace Cike.Workflow.Application.Folders.Validators;

public class AddFolderDtoValidator : AbstractValidator<AddFolderDto>
{
    public AddFolderDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("目录名称不能为空。")
            .MaximumLength(64).WithMessage("目录名称不能超过64个字符。")
            .Matches(@"^[一-龥a-zA-Z0-9_-]+$").WithMessage("目录名称只支持中文、英文字母、数字、下划线和连字符。");
    }
}
