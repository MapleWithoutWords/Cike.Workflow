using Cike.Workflow.Common.Versions;

namespace Cike.Workflow.Core.Models;

public class WorkflowDefinitionHandle
{
    public string? DefinitionId { get; set; }

    public VersionOptions? VersionOptions { get; set; }

    public long? DefinitionVersionId { get; set; }

    public static WorkflowDefinitionHandle ByDefinitionId(string definitionId, VersionOptions? versionOptions = null) => new() { DefinitionId = definitionId, VersionOptions = versionOptions };

    public static WorkflowDefinitionHandle ByDefinitionVersionId(long definitionVersionId) => new() { DefinitionVersionId = definitionVersionId };

    public override string ToString()
    {
        if (DefinitionId != null)
            return $"DefinitionId: {DefinitionId}, VersionOptions: {VersionOptions}";

        if (DefinitionVersionId != null)
            return $"DefinitionVersionId: {DefinitionVersionId}";

        return string.Empty;
    }
}
