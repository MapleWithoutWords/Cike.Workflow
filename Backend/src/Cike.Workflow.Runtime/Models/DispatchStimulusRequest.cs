namespace Cike.Workflow.Runtime.Models;

public class DispatchStimulusRequest
{
    public string? ActivityTypeName { get; set; }
    public BaseStimulus? Stimulus { get; set; }
    public string? StimulusHash { get; set; }
    public StimulusMetadata? Metadata { get; set; }
}
