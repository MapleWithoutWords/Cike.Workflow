namespace Cike.Workflow.Domain.Filters;

public class TimestampFilter
{
    public required string Column { get; set; } = null!;

    public required TimestampFilterOperator Operator { get; set; }

    public required DateTime Timestamp { get; set; }
}

public enum TimestampFilterOperator
{
    Is,

    IsNot,

    LessThan,

    GreaterThan,

    LessThanOrEqual,

    GreaterThanOrEqual
}
