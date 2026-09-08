using Cike.Workflow.Application.Contracts.WorkflowDefinitions;
using FluentValidation;

namespace Cike.Workflow.Application.WorkflowDefinitions.Validators;

public class AddWorkflowDefinitionDtoValidator : AbstractValidator<AddWorkflowDefinitionDto>
{
    public AddWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("工作流名称不能为空。")
            .MaximumLength(64).WithMessage("工作流名称不能超过64个字符。")
            .Matches(@"^[一-龥a-zA-Z0-9_-]+$").WithMessage("工作流名称只支持中文、英文字母、数字、下划线和连字符。");
    }
}
