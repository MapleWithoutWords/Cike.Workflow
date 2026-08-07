namespace Cike.Workflow.Core.Activities.Abstracts;

public interface IActivity
{
    string Id { get; set; }

    /// <summary>
    /// 节点Path
    /// </summary>
    string IdPath { get; set; }

    public string Code { get; set; }

    string? Name { get; set; }

    string Type { get; set; }

    int Version { get; set; }

    IDictionary<string, object> CustomProperties { get; set; }

    IDictionary<string, object> Metadata { get; set; }

    ValueTask<bool> CanExecuteAsync(ActivityExecutionContext context);

    ValueTask ExecuteAsync(ActivityExecutionContext context);
}
