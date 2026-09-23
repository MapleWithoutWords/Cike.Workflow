using Cike.Workflow.Common.Serialization.Internals;
using Cike.Workflow.Core.Contexts;

namespace Cike.Workflow.Core.Runners.Internals;

/// <summary>
/// Materializes default values of workflow-level input/output definitions:
/// input defaults when the caller did not provide the input (first start),
/// output defaults when the workflow finished without writing the output.
/// </summary>
internal class WorkflowArgumentDefaultMaterializer(
    IExpressionEvaluator expressionEvaluator,
    ISerializationTypeRegistry serializationTypeRegistry,
    ILogger<WorkflowArgumentDefaultMaterializer> logger) : IScopedDependency
{
    public async Task MaterializeInputDefaultsAsync(WorkflowExecutionContext context)
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

    public async Task MaterializeOutputDefaultsAsync(WorkflowExecutionContext context)
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
