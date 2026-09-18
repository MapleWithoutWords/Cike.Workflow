using Cike.Workflow.Expressions.Extensions;
using System.Net.Http.Headers;

namespace Cike.Workflow.Http.Models;

/// <summary>
/// Represents the headers of an HTTP message.
/// </summary>
public class CustomHttpHeaders : Dictionary<string, string[]>
{
    /// <inheritdoc />
    public CustomHttpHeaders()
    {
    }

    /// <inheritdoc />
    public CustomHttpHeaders(IDictionary<string, string[]> source)
    {
        foreach (var item in source)
            Add(item.Key, item.Value);
    }

    /// <inheritdoc />
    public CustomHttpHeaders(HttpResponseHeaders source)
    {
        foreach (var item in source)
            Add(item.Key, item.Value.ToArray());
    }

    /// <inheritdoc />
    public CustomHttpHeaders(HttpContentHeaders source)
    {
        foreach (var item in source)
            Add(item.Key, item.Value.ToArray());
    }

    /// <summary>
    /// Gets the content type.
    /// </summary>
    public string? ContentType => this.GetValue("content-type")?[0];
}
