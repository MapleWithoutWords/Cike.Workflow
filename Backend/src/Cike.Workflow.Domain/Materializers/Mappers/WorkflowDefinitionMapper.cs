namespace Cike.Workflow.Domain.Materializers.Mappers;

public class WorkflowDefinitionMapper : ISingletonDependency
{
    private readonly VariableDefinitionMapper _variableDefinitionMapper;

    public WorkflowDefinitionMapper(VariableDefinitionMapper variableDefinitionMapper)
    {
        _variableDefinitionMapper = variableDefinitionMapper;
    }

    public WorkflowActivity Map(WorkflowDefinition source)
    {
        var root = JsonHelper.Deserialize<Activity>(source.OriginalStringData!);

        var variables = source.Options?.Variables?.Select(v => _variableDefinitionMapper.Map(v)).Where(e => e != null).ToList() ?? new List<Core.Variables.Variable>();

        return new(root,
            variables!,
            source.Options!.Inputs,
            source.Options.Outputs,
            source.Options.Outcomes,
            source.Options.CustomProperties,
            source.IsReadonly,
            source.IsSystem,
            new WorkflowDefinitionInfo()
            {
                Id = source.Id,
                DefinitionId = source.DefinitionId,
                IsReadonly = source.IsReadonly,
                Description = source.Description,
                IsLatest = source.IsLatest,
                IsPublished = source.IsPublished,
                Name = source.Name,
                TenantId = source.TenantId,
                UsableAsActivity = source.UsableAsActivity,
                Version = source.Version
            });
    }
}
