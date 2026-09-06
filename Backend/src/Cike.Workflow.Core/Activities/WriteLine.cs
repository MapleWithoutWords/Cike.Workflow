namespace Cike.Workflow.Core.Activities;

/// <summary>
///  Write a line of text to the console.
/// </summary>
[Activity("Cike", "Console", "Write a line of text to the console.")]
public class WriteLine : AutoCompleteActivity
{
    /// <inheritdoc />
    [JsonConstructor]
    private WriteLine() : base()
    {
    }

    /// <inheritdoc />
    public WriteLine(string text) : this(new Literal<string>(text))
    {
    }

    /// <inheritdoc />
    public WriteLine(Variable<string> variable) : this() => Text = new Input<string>(variable);

    /// <inheritdoc />
    public WriteLine(Literal<string> literal) : this() => Text = new Input<string>(literal);

    /// <inheritdoc />
    public WriteLine(Expression expression) : this() => Text = new Input<string>(expression, new MemoryBlockReference());

    /// <inheritdoc />
    public WriteLine(Input<string> text) : this() => Text = text;

    /// <summary>
    /// The text to write.
    /// </summary>
    [Description("The text to write.")]
    public Input<string> Text { get; set; } = null!;

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var text = context.Get(Text);
        Console.WriteLine(text);
    }
}
