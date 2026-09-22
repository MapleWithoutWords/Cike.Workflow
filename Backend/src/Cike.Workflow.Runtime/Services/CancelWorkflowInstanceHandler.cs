using Cike.EventBus.Local;
using Cike.Workflow.Core.Runners;
using Cike.Workflow.Core.Runners.Internals.Commands;
using Cike.Workflow.Domain.Managers;

namespace Cike.Workflow.Runtime.Internals;

/// <summary>
/// 取消命令处理器：上下文置为取消、提取状态并落库。
/// 由 WorkflowClient 发布（构建好上下文后发 RunCancelWorkflowCommand），
/// 推送中间件据此补推取消终态——取消不经过运行命令，需独立进总线。
/// </summary>
internal class CancelWorkflowInstanceHandler(
    WorkflowInstanceManager workflowInstanceManager,
    IWorkflowStateExtractor workflowStateExtractor)
{
    [LocalEventHandler]
    public async Task RunCancelWorkflowAsync(RunCancelWorkflowCommand command, CancellationToken cancellationToken)
    {
        command.Context.Cancel();
        var workflowState = workflowStateExtractor.Extract(command.Context);
        await workflowInstanceManager.SaveAsync(workflowState, cancellationToken);
    }
}
