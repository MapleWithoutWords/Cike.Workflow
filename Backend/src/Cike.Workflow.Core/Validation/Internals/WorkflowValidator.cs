using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Activities.FlowchartActivity;

namespace Cike.Workflow.Core.Validation.Internals;

/// <summary>
/// 画布严格校验默认实现：开始节点存在、无孤立节点、连线引用完整、变量定义合法、活动必填属性齐全。
/// </summary>
public class WorkflowValidator : IWorkflowValidator, ISingletonDependency
{
    public List<WorkflowValidationError> Validate(WorkflowValidationContext context)
    {
        var errors = new List<WorkflowValidationError>();
        var flowchart = context.Root as Flowchart;

        if (flowchart == null)
        {
            errors.Add(new(null, "画布根节点必须是流程图（Flowchart）。"));
            return errors;
        }

        ValidateStartNode(flowchart, errors);
        ValidateConnections(flowchart, errors);
        ValidateOrphanNodes(flowchart, errors);
        ValidateRequiredInputs(flowchart, errors);
        ValidateVariables(context.Variables, errors);

        return errors;
    }

    /// <summary>开始节点存在：显式 Start、Start 类型活动、或标记可启动工作流的活动，三者居一。</summary>
    private static void ValidateStartNode(Flowchart flowchart, List<WorkflowValidationError> errors)
    {
        var hasStart = flowchart.Start != null
                       || flowchart.Activities.Any(x => x is Start)
                       || flowchart.Activities.Any(x => x.GetCanStartWorkflow());

        if (!hasStart)
            errors.Add(new(flowchart.Start?.Id, "画布中未找到开始节点，请添加开始节点后再发布。"));
    }

    private static void ValidateConnections(Flowchart flowchart, List<WorkflowValidationError> errors)
    {
        var activityIds = flowchart.Activities.Select(x => x.Id).ToHashSet();

        foreach (var connection in flowchart.Connections)
        {
            if (!activityIds.Contains(connection.Source.ActivityId))
                errors.Add(new(connection.Source.ActivityId, $"连线引用了不存在的源节点 [{connection.Source.ActivityId}]。"));
            if (!activityIds.Contains(connection.Target.ActivityId))
                errors.Add(new(connection.Target.ActivityId, $"连线引用了不存在的目标节点 [{connection.Target.ActivityId}]。"));
        }
    }

    /// <summary>无孤立节点：除开始节点外，每个节点必须至少参与一条连线（有入边或出边）。</summary>
    private static void ValidateOrphanNodes(Flowchart flowchart, List<WorkflowValidationError> errors)
    {
        var connectedIds = flowchart.Connections
            .SelectMany(x => new[] { x.Source.ActivityId, x.Target.ActivityId })
            .ToHashSet();

        foreach (var activity in flowchart.Activities)
        {
            if (connectedIds.Contains(activity.Id))
                continue;

            // 仅含开始节点的空流程合法：开始后流程直接完成
            if (activity is Start || activity.GetCanStartWorkflow() || ReferenceEquals(activity, flowchart.Start))
                continue;

            errors.Add(new(activity.Id, $"节点 [{activity.Id}] 是孤立节点，请将其接入流程或删除。"));
        }
    }

    /// <summary>活动必填属性齐全：输入属性（Input 派生）不允许为空，容器活动递归校验。</summary>
    private static void ValidateRequiredInputs(IActivity activity, List<WorkflowValidationError> errors)
    {
        foreach (var property in activity.GetType().GetProperties())
        {
            if (!typeof(Input).IsAssignableFrom(property.PropertyType))
                continue;

            if (property.GetValue(activity) == null)
                errors.Add(new(activity.Id, $"节点 [{activity.Id}] 缺少必填属性 [{property.Name}]。"));
        }

        if (activity is ContainerActivity container)
        {
            foreach (var child in container.Activities)
                ValidateRequiredInputs(child, errors);
        }
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
