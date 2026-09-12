using Cike.Workflow.Domain.Materializers.Mappers;

namespace Cike.Workflow.Domain.Materializers.Internals;

/// <summary>
/// A workflow materializer that deserializes workflows created in C# code.
/// </summary>
public class TypedWorkflowMaterializer(WorkflowDefinitionMapper workflowDefinitionMapper) : IWorkflowMaterializer, IScopedDependency
{
    /// <summary>
    /// The name of the materializer.
    /// </summary>
    public const string MaterializerName = "Typed";

    /// <inheritdoc />
    public string Name => MaterializerName;

    /// <inheritdoc />
    public ValueTask<WorkflowActivity> MaterializeAsync(WorkflowDefinition definition, CancellationToken cancellationToken)
    {
        var workflow = ToWorkflow(definition);
        return new ValueTask<WorkflowActivity>(workflow);
    }

    private WorkflowActivity ToWorkflow(WorkflowDefinition definition) => workflowDefinitionMapper.Map(definition);
}
