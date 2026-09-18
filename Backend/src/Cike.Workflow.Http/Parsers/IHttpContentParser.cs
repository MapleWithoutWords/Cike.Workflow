namespace Cike.Workflow.Http.Parsers;

public interface IHttpContentParser
{
    int Priority { get; }

    bool GetSupportsContentType(HttpResponseParserContext context);

    Task<object> ReadAsync(HttpResponseParserContext context);
}
