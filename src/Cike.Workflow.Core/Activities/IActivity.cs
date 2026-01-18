using Cike.Workflow.Core.Contexts;

namespace Cike.Workflow.Core.Activities;

public interface IActivity
{
    public long Id { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    int Version { get; set; }

    public ValueTask ExecuteAsync(ActivityExecutionContext context, CancellationToken cancellationToken = default);
}
