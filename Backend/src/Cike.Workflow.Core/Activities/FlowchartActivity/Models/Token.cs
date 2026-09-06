namespace Cike.Workflow.Core.Activities.FlowchartActivity.Models;

internal class Token(string fromActivityId, string? fromActivityName, string? outcome, string toActivityId, string? toActivityName, bool consumed, bool blocked)
{
    public static Token Create(string fromId, string toId, string? outcome) => new(fromId, null, outcome, toId, null, false, false);

    public string FromActivityId { get; } = fromActivityId;
    public string? FromActivityName { get; } = fromActivityName;
    public string? Outcome { get; } = outcome;
    public string ToActivityId { get; } = toActivityId;
    public string? ToActivityName { get; } = toActivityName;
    public bool Consumed { get; private set; } = consumed;
    public bool Blocked { get; private set; } = blocked;

    public Token Consume()
    {
        Consumed = true;
        return this;
    }

    public Token Block()
    {
        Blocked = true;
        return this;
    }
}
