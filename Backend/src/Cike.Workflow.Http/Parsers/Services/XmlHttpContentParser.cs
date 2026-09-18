namespace Cike.Workflow.Http.Parsers.Services;

public class XmlHttpContentParser : IHttpContentParser, ISingletonDependency
{
    public int Priority => 0;

    public bool GetSupportsContentType(HttpResponseParserContext context) => context.ContentType.Contains("xml", StringComparison.InvariantCultureIgnoreCase);

    public async Task<object> ReadAsync(HttpResponseParserContext context)
    {
        var content = context.Content;
        using var reader = new StreamReader(content, leaveOpen: true);
        var xml = await reader.ReadToEndAsync();
        var returnType = context.ReturnType;

        if (returnType == null || returnType == typeof(string))
            return xml;

        var serializer = new XmlSerializer(returnType);
        return serializer.Deserialize(reader)!;
    }
}
