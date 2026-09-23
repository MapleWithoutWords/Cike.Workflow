using Cike.EventBus.Local.Enums;
using Cike.EventBus.Local.LocalEventMiddlewares;
using Cike.Workflow.Common.Serialization.Internals;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Exceptions;
using Cike.Workflow.Core.Runners.Internals.Commands;

namespace Cike.Workflow.Core.Runners.Internals.Middlewares;

/// <summary>
/// Materializes workflow argument (input/output) default values around the instance run and
/// performs the terminal status transition:
/// - before the run (first start only): evaluate defaults for inputs the caller did not provide;
/// - after the scheduler drains: evaluate defaults for outputs that were never written, then
///   transition the instance to Finished/Suspended. An evaluation failure propagates to the
///   exception middleware which transitions the workflow to Faulted.
/// Downstream status observers must derive terminal state from the completion fact
/// (all activity execution contexts completed) rather than from the status flag alone.
/// </summary>
internal class WorkflowArgumentDefaultMiddleware(
    IExpressionEvaluator expressionEvaluator,
    ISerializationTypeRegistry serializationTypeRegistry,
    ILogger<WorkflowArgumentDefaultMiddleware> logger) : ILocalEventMiddleware<RunWorkflowInstanceCommand>
{
    // Always (not OncePerTree): nested child-workflow runs are nested publishes and must get
    // defaults and the terminal transition just like root runs.
    public MiddlewareExecutionPolicy ExecutionPolicy => MiddlewareExecutionPolicy.Always;

    public async Task HandleAsync(RunWorkflowInstanceCommand @event, EventHandlerDelegate next)
    {
        var context = @event.Context;

        if (@event.IsStarting)
            await MaterializeInputDefaultsAsync(context);

        await next();

        if (context.Status.GetMainStatus() == WorkflowMainStatus.Running)
        {
            var isFinished = context.ActivityExecutionContexts.All(x => x.IsCompleted);

            if (isFinished)
                await MaterializeOutputDefaultsAsync(context);

            context.TransitionTo(isFinished ? WorkflowStatus.Finished : WorkflowStatus.Suspended);
        }
    }

    private async Task MaterializeInputDefaultsAsync(WorkflowExecutionContext context)
    {
        foreach (var inputDefinition in context.Workflow.Inputs)
        {
            if (context.Input.ContainsKey(inputDefinition.Name))
                continue;

            if (!HasConfiguredDefault(inputDefinition))
                continue;

            // Input defaults only allow Literal/Liquid/JavaScript (no input/variable references),
            // so the bare workflow-level expression context is sufficient to evaluate them.
            context.Input[inputDefinition.Name] = await EvaluateDefaultAsync(inputDefinition, context.ExpressionExecutionContext);
        }
    }

    private async Task MaterializeOutputDefaultsAsync(WorkflowExecutionContext context)
    {
        // Output defaults may reference workflow inputs and variables. The WorkflowInput/Variable
        // handlers can only resolve on an expression context linked to an activity execution context
        // (GetInput relies on the back-reference), so evaluate on the root activity context.
        var rootActivityContext = context.ActivityExecutionContexts.FirstOrDefault(x => x.ParentActivityExecutionContext == null);
        var expressionContext = rootActivityContext?.ExpressionExecutionContext ?? context.ExpressionExecutionContext;

        foreach (var outputDefinition in context.Workflow.Outputs)
        {
            if (context.Output.ContainsKey(outputDefinition.Name))
                continue;

            if (!HasConfiguredDefault(outputDefinition))
                continue;

            context.Output[outputDefinition.Name] = await EvaluateDefaultAsync(outputDefinition, expressionContext);
        }
    }

    /// <summary>
    /// The model default of <see cref="ArgumentDefinition.DefaultValue"/> is a null literal placeholder,
    /// which means "no default configured". Treating it as absent keeps unconfigured inputs/outputs
    /// missing (rather than materialized as null) and preserves the pre-existing observable behavior.
    /// </summary>
    private static bool HasConfiguredDefault(ArgumentDefinition definition) =>
        definition.DefaultValue is not { Type: "Literal", Value: null };

    private async ValueTask<object?> EvaluateDefaultAsync(ArgumentDefinition definition, ExpressionExecutionContext expressionContext)
    {
        try
        {
            var returnType = ResolveReturnType(definition);
            return await expressionEvaluator.EvaluateAsync(definition.DefaultValue, returnType, expressionContext);
        }
        catch (Exception e)
        {
            throw new ArgumentEvaluationException(definition.Name, $"Failed to evaluate the default value of the workflow argument '{definition.Name}'.", e);
        }
    }

    private Type ResolveReturnType(ArgumentDefinition definition)
    {
        if (!SerializationTypeResolver.TryResolveType(serializationTypeRegistry, definition.Type, out var elementType))
        {
            logger.LogWarning("Failed to resolve the type {TypeAlias} of the workflow argument {ArgumentName}. Falling back to Object.", definition.Type, definition.Name);
            return typeof(object);
        }

        return definition.IsArray ? elementType.MakeArrayType() : elementType;
    }
}
