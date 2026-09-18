namespace Cike.Workflow.Common.Versions;

public interface IVersion
{
    public int Version { get; set; }

    public bool IsLatest { get; set; }

    public bool IsPublished { get; set; }
}
