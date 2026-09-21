using Cike.Workflow.Application.Contracts.WorkflowDefinitions;
using FluentValidation;

namespace Cike.Workflow.Application.WorkflowDefinitions.Validators;

public class ValidateWorkflowCanvasDtoValidator : AbstractValidator<ValidateWorkflowCanvasDto>
{
    public ValidateWorkflowCanvasDtoValidator()
    {
        RuleFor(x => x.Root)
            .NotNull().WithMessage("画布内容不能为空。");
    }
}
