namespace Cike.Workflow.Core.Contexts;

public record ActivityCompletedContext(ActivityExecutionContext TargetContext, ActivityExecutionContext ChildContext, object? Result = default)
{
    public WorkflowExecutionContext WorkflowExecutionContext => TargetContext.WorkflowExecutionContext;

    public T GetRequiredService<T>() where T : notnull => WorkflowExecutionContext.GetRequiredService<T>();

    public CancellationToken CancellationToken => WorkflowExecutionContext.CancellationToken;

    [RequiresUnreferencedCode("The activity may be serialized and executed in a different context.")]
    public async ValueTask CompleteActivityAsync(Outcomes? result = default)
    {
        await TargetContext.CompleteActivityAsync(result);
    }

    [RequiresUnreferencedCode("The activity may be serialized and executed in a different context.")]
    public ValueTask CompleteActivityWithOutcomesAsync(params string[] outcomes) => CompleteActivityAsync(new Outcomes(outcomes));
}
