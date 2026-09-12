using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.Serialization;
using Cike.Workflow.Domain.Shared.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Cike.EntityFrameworkCore.Stores;

public class ActivityInstanceExecutionRecordStore(CikeWorkflowDbContenxt context, IPayloadSerializer payloadSerializer, ILogger<ActivityInstanceExecutionRecordStore> logger)
    : BaseStore<ActivityInstanceExecutionRecord>(context), IActivityInstanceExecutionRecordStore
{
    public override async Task AddRangeAsync(IEnumerable<ActivityInstanceExecutionRecord> entities, CancellationToken cancellationToken = default)
    {
        foreach (var item in entities)
        {
            await OnSaveAsync(item, cancellationToken);
        }
        await base.AddRangeAsync(entities, cancellationToken);
    }

    private ValueTask OnSaveAsync(ActivityInstanceExecutionRecord entity, CancellationToken cancellationToken)
    {
        context.Entry(entity).Property("SerializedActivityState").CurrentValue = entity.ActivityState?.Any() == true ? payloadSerializer.Serialize(entity.ActivityState) : null;
        context.Entry(entity).Property("SerializedPayload").CurrentValue = entity.Payload?.Any() == true ? payloadSerializer.Serialize(entity.Payload) : null;
        context.Entry(entity).Property("SerializedOutputs").CurrentValue = entity.Outputs?.Any() == true ? payloadSerializer.Serialize(entity.Outputs) : null;
        context.Entry(entity).Property("SerializedProperties").CurrentValue = entity.Properties?.Any() == true ? payloadSerializer.Serialize(entity.Properties) : null;
        context.Entry(entity).Property("SerializedMetadata").CurrentValue = entity.Metadata?.Any() == true ? payloadSerializer.Serialize(entity.Metadata) : null;
        context.Entry(entity).Property("SerializedException").CurrentValue = entity.Exception != null ? payloadSerializer.Serialize(entity.Exception) : null;
        return ValueTask.CompletedTask;
    }

    public override async Task<ActivityInstanceExecutionRecord?> FindAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await base.FindAsync(id, cancellationToken);
        await OnLoadAsync(result, cancellationToken);
        return result;
    }

    private ValueTask OnLoadAsync(ActivityInstanceExecutionRecord? entity, CancellationToken cancellationToken)
    {
        if (entity == null)
            return ValueTask.CompletedTask;

        try
        {
            var activityStateJson = (string?)context.Entry(entity).Property("SerializedActivityState").CurrentValue;
            var payloadJson = (string?)context.Entry(entity).Property("SerializedPayload").CurrentValue;
            var outputJson = (string?)context.Entry(entity).Property("SerializedOutputs").CurrentValue;
            var propertiesJson = (string?)context.Entry(entity).Property("SerializedProperties").CurrentValue;
            var metadataJson = (string?)context.Entry(entity).Property("SerializedMetadata").CurrentValue;
            var exceptionJson = (string?)context.Entry(entity).Property("SerializedException").CurrentValue;

            entity.ActivityState = activityStateJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<IDictionary<string, object?>?>(activityStateJson);
            entity.Payload = payloadJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<IDictionary<string, object>?>(payloadJson);
            entity.Outputs = outputJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<IDictionary<string, object?>?>(outputJson);
            entity.Properties = propertiesJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<IDictionary<string, object>?>(propertiesJson);
            entity.Metadata = metadataJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<IDictionary<string, object>?>(metadataJson);
            entity.Exception = exceptionJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<ExceptionState?>(exceptionJson);
        }
        catch (Exception exp)
        {
            logger.LogError(exp, "无法反序列化执行记录：{Id}", entity.Id);
        }

        return ValueTask.CompletedTask;
    }
}
