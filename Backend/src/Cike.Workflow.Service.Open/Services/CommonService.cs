using Cike.Workflow.Common.Serialization;
using Cike.Workflow.Core.ActivityDescriptors.Models;
using Cike.Workflow.Core.StorageDrivers;
using Cike.Workflow.Core.StorageDrivers.Models;
using System.ComponentModel;

namespace Cike.Workflow.Service.Open.Services;

public class CommonService : MinimalApiServiceBase
{
    public async Task<Results<Ok<List<ExpressionDescriptor>>, BadRequest>> GetExpressionDescriptorsAsync([FromServices] IExpressionDescriptorRegistry expressionDescriptorRegistry)
    {
        var result = expressionDescriptorRegistry.ListAll();
        return TypedResults.Ok(result.ToList());
    }

    public async Task<Results<Ok<List<StorageDriverDescriptor>>, BadRequest>> GetStorageDriverDescriptorsAsync([FromServices] IStorageDriverRegistry storageDriverRegistry)
    {
        var result = storageDriverRegistry.ListAll();
        return TypedResults.Ok(result.ToList());
    }

    public async Task<Results<Ok<IEnumerable<ActivityDescriptor>>, BadRequest>> GetActivityDescriptorsAsync([FromServices] IActivityRegistry activityRegistry)
    {
        return TypedResults.Ok(activityRegistry.ListAll());
    }

    public async Task<Results<Ok<List<VariableTypeDescriptor>>, BadRequest>> GetVarialbeTypesAsync([FromServices] ISerializationTypeRegistry serializationTypeRegistry)
    {
        var result = new List<VariableTypeDescriptor>();
        foreach (var item in serializationTypeRegistry.ListTypes())
        {
            var hasAlias = serializationTypeRegistry.TryGetAlias(item, out var alias);
            var typeName = hasAlias ? alias : item.GetSimpleAssemblyQualifiedName()!;
            var displayName = hasAlias ? alias : item.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? item.Name;
            result.Add(new(typeName, displayName));
        }
        return TypedResults.Ok(result);
    }
}

public record VariableTypeDescriptor(string TypeName, string DisplayName);
