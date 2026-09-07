using Cike.Workflow.Core.Models;

namespace Cike.Workflow.Domain.Data.ValueObjects;

public class WorkflowDefinitionOptionsValueObject
{
    public List<VariableDefinition> Variables { get; set; } = new List<VariableDefinition>();

    public List<InputDefinition> Inputs { get; set; } = new List<InputDefinition>();

    public List<OutputDefinition> Outputs { get; set; } = new List<OutputDefinition>();

    public List<string> Outcomes { get; set; } = new List<string>();

    public Dictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();
}
