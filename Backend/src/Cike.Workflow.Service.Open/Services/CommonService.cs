namespace Cike.Workflow.Service.Open.Services;

public class CommonService : MinimalApiServiceBase
{
    public async Task<Results<Ok<List<ExpressionDescriptor>>, BadRequest>> GetExpressionDescriptorsAsync([FromServices] IExpressionDescriptorRegistry expressionDescriptorRegistry)
    {
        var result = expressionDescriptorRegistry.ListAll();
        return TypedResults.Ok(result.ToList());
    }
}
