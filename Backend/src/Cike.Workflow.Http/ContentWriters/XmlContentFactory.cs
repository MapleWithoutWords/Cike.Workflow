namespace Cike.Workflow.Http.ContentWriters;

/// <summary>
/// Creates a <see cref="HttpContent"/> object for XML types.
/// </summary>
public class XmlContentFactory : IHttpContentFactory, IScopedDependency
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedContentTypes => new[]
    {
        MediaTypeNames.Application.Xml,
        MediaTypeNames.Text.Xml,
        MediaTypeNames.Application.Soap,
    };

    /// <inheritdoc />
    public HttpContent CreateHttpContent(object content, string contentType)
    {
        var text = content as string ?? Serialize(content);
        return new RawStringContent(text, Encoding.UTF8, contentType);
    }

    private string Serialize(object value)
    {
        using var writer = new StringWriter();
        new XmlSerializer(value.GetType()).Serialize(writer, value);
        return writer.ToString();
    }
}
