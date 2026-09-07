using Cike.Core.DependencyInjection;
using Cike.Workflow.Domain.Materializers.Mappers;

namespace Cike.Workflow.Domain.Materializers.Internals;

/// <summary>
/// Materializes a <see cref="Workflow"/> from a <see cref="WorkflowDefinition"/>'s JSON data.
/// </summary>
public class JsonWorkflowMaterializer : IWorkflowMaterializer, IScopedDependency
{
    private readonly WorkflowDefinitionMapper _workflowDefinitionMapper;

    /// <summary>
    /// The name of the materializer.
    /// </summary>
    public const string MaterializerName = "Json";

    /// <inheritdoc />
    public string Name => MaterializerName;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonWorkflowMaterializer"/> class.
    /// </summary>
    public JsonWorkflowMaterializer(WorkflowDefinitionMapper workflowDefinitionMapper)
    {
        _workflowDefinitionMapper = workflowDefinitionMapper;
    }

    /// <inheritdoc />
    public ValueTask<WorkflowActivity> MaterializeAsync(WorkflowDefinition definition, CancellationToken cancellationToken)
    {
        var workflow = ToWorkflow(definition);
        return new(workflow);
    }

    private WorkflowActivity ToWorkflow(WorkflowDefinition definition) => _workflowDefinitionMapper.Map(definition);
}
