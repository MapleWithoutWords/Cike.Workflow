namespace Cike.Workflow.Application.Folders.Validators;

public class MoveFolderDtoValidator : AbstractValidator<MoveFolderDto>
{
    public MoveFolderDtoValidator()
    {
        RuleFor(x => x.ParentId)
            .GreaterThanOrEqualTo(0).WithMessage("目标目录不能为负数。");
    }
}
