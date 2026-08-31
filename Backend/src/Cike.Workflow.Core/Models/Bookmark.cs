using System.Text.Json.Serialization;

namespace Cike.Workflow.Core.Models;

/// <summary>
/// A bookmark represents a location in a workflow where the workflow can be resumed at a later time.
/// </summary>
/// <param name="id">The ID of the bookmark.</param>
/// <param name="name">The name of the bookmark.</param>
/// <param name="hash">The hash of the bookmark.</param>
/// <param name="payload">The data associated with the bookmark.</param>
/// <param name="activityId">The ID of the activity associated with the bookmark.</param>
/// <param name="activityNodeId">The ID of the activity node associated with the bookmark.</param>
/// <param name="activityInstanceId">The ID of the activity instance associated with the bookmark.</param>
/// <param name="createdAt">The date and time the bookmark was created.</param>
/// <param name="autoBurn">Whether or not the bookmark should be automatically burned.</param>
/// <param name="callbackMethodName">The name of the method on the activity class to invoke when the bookmark is resumed.</param>
/// <param name="autoComplete">Whether or not the activity should be automatically completed when the bookmark is resumed.</param>
/// <param name="metadata">Custom properties associated with the bookmark.</param>
public class Bookmark(
    long id,
    string name,
    string hash,
    object? payload,
    string activityId,
    string activityNodeId,
    long? activityInstanceId,
    DateTimeOffset createdAt,
    bool autoBurn = true,
    string? callbackMethodName = null,
    bool autoComplete = true,
    IDictionary<string, string>? metadata = null)
{
    /// <inheritdoc />
    [JsonConstructor]
    public Bookmark() : this(0, "", "", null, "", "", 0, default, false)
    {
    }

    public long Id { get; set; } = id;

    public string Name { get; set; } = name;

    public string Hash { get; set; } = hash;

    public object? Payload { get; set; } = payload;

    public string ActivityId { get; set; } = activityId;

    public string ActivityNodeId { get; set; } = activityNodeId;

    public long? ActivityInstanceId { get; set; } = activityInstanceId;

    public DateTimeOffset CreatedAt { get; set; } = createdAt;

    public bool AutoBurn { get; set; } = autoBurn;

    public string? CallbackMethodName { get; set; } = callbackMethodName;

    public bool AutoComplete { get; set; } = autoComplete;

    public IDictionary<string, string>? Metadata { get; set; } = metadata;
}
