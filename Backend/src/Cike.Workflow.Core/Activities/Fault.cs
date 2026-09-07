namespace Cike.Workflow.Core.Activities;

/// <summary>
/// Faults the workflow.
/// </summary>
[Activity("Cike", "Primitives", "Faults the workflow.")]
public class Fault : Activity
{
    /// <inheritdoc />
    public Fault() : base()
    {
    }

    /// <summary>
    /// Creates a fault activity.
    /// </summary>
    public static Fault Create(string code, string category, string type, string? message = null)
    {
        return new()
        {
            Code = new(code),
            Message = new(message),
            Category = new(category),
            FaultType = new(type)
        };
    }

    /// <summary>
    /// Code to categorize the fault.
    /// </summary>
    [Input(Description = "Code to categorize the fault.")]
    public Input<string> Code { get; set; } = null!;

    /// <summary>
    /// Category to categorize the fault. Examples: HTTP, Alteration, Azure, etc.
    /// </summary>
    [Input(Description = "Category to categorize the fault. Examples: HTTP, Alteration, Azure, etc.")]
    public Input<string> Category { get; set; } = null!;

    /// <summary>
    /// The type of fault. Examples: System, Business, Integration, etc.
    /// </summary>
    [Input(
        DisplayName = "Type",
        Description = "The type of fault. Examples: System, Business, Integration, etc."
        )]
    public Input<string> FaultType { get; set; } = null!;

    /// <summary>
    /// The message to include with the fault.
    /// </summary>
    [Input(Description = "The message to include with the fault.")]
    public Input<string?> Message { get; set; } = null!;

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var code = Code.GetOrDefault(context) ?? "0";
        var category = Category.GetOrDefault(context) ?? "General";
        var type = FaultType.GetOrDefault(context) ?? "System";
        var message = Message.GetOrDefault(context);
        throw new FaultException(code, category, type, message);
    }
}
