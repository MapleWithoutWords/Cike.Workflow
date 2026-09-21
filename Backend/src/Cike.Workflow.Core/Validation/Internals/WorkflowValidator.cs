using Cike.Workflow.Core.Activities.FlowchartActivity;

namespace Cike.Workflow.Core.Validation.Internals;

/// <summary>
/// 画布严格校验触发器：画布根必须是 Flowchart、根活动自校验（递归整棵树）、变量定义合法。
/// 各活动的具体校验规则见 Activity.Validate 及其覆写。
/// </summary>
public class WorkflowValidator : IWorkflowValidator, ISingletonDependency
{
    public List<WorkflowValidationError> Validate(WorkflowValidationContext context)
    {
        if (context.Root is not Flowchart)
        {
            context.Errors.Add(new(null, "画布根节点必须是流程图（Flowchart）。"));
            return context.Errors;
        }

        context.Root.Validate(context);
        ValidateVariables(context.Variables, context.Errors);

        return context.Errors;
    }

    private static void ValidateVariables(IReadOnlyList<WorkflowVariableDefinition> variables, List<WorkflowValidationError> errors)
    {
        for (var i = 0; i < variables.Count; i++)
        {
            var variable = variables[i];
            if (variable.Name.IsNullOrWhiteSpace())
                errors.Add(new(null, $"第 {i + 1} 个变量定义的名称不能为空。"));
            if (variable.TypeName.IsNullOrWhiteSpace())
                errors.Add(new(null, $"变量 [{variable.Name}] 的类型不能为空。"));
        }

        var duplicateNames = variables
            .Where(x => !x.Name.IsNullOrWhiteSpace())
            .GroupBy(x => x.Name)
            .Where(g => g.Count() > 1);
        foreach (var group in duplicateNames)
            errors.Add(new(null, $"变量名称 [{group.Key}] 重复，请修改后重试。"));
    }
}
