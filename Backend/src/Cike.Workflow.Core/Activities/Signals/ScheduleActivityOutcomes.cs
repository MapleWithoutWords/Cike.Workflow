namespace Cike.Workflow.Core.Activities.Signals;

/// <summary>
/// Signaled when an activity requests the scheduling of the specified set of outcomes.
/// </summary>
/// <param name="Outcomes">The outcomes to schedule.</param>
public record ScheduleActivityOutcomesSignal(params string[] Outcomes);
