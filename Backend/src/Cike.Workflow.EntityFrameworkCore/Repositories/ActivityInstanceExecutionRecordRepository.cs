using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.Serialization;
using Cike.Workflow.Domain.Shared.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Cike.EntityFrameworkCore.Repositories;

public class ActivityInstanceExecutionRecordRepository(CikeWorkflowDbContext context, IPayloadSerializer payloadSerializer, ILogger<ActivityInstanceExecutionRecordRepository> logger)
    : SerializedEfCoreRepository<CikeWorkflowDbContext, ActivityInstanceExecutionRecord>(context), IActivityInstanceExecutionRecordRepository, IScopedDependency
{
    protected override ValueTask OnSaveAsync(ActivityInstanceExecutionRecord entity, CancellationToken cancellationToken)
    {
        DbContext.Entry(entity).Property("SerializedActivityState").CurrentValue = entity.ActivityState?.Any() == true ? payloadSerializer.Serialize(entity.ActivityState) : null;
        DbContext.Entry(entity).Property("SerializedPayload").CurrentValue = entity.Payload?.Any() == true ? payloadSerializer.Serialize(entity.Payload) : null;
        DbContext.Entry(entity).Property("SerializedOutputs").CurrentValue = entity.Outputs?.Any() == true ? payloadSerializer.Serialize(entity.Outputs) : null;
        DbContext.Entry(entity).Property("SerializedProperties").CurrentValue = entity.Properties?.Any() == true ? payloadSerializer.Serialize(entity.Properties) : null;
        DbContext.Entry(entity).Property("SerializedMetadata").CurrentValue = entity.Metadata?.Any() == true ? payloadSerializer.Serialize(entity.Metadata) : null;
        DbContext.Entry(entity).Property("SerializedException").CurrentValue = entity.Exception != null ? payloadSerializer.Serialize(entity.Exception) : null;
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnLoadAsync(ActivityInstanceExecutionRecord? entity, CancellationToken cancellationToken)
    {
        if (entity == null)
            return ValueTask.CompletedTask;

        try
        {
            var activityStateJson = (string?)DbContext.Entry(entity).Property("SerializedActivityState").CurrentValue;
            var payloadJson = (string?)DbContext.Entry(entity).Property("SerializedPayload").CurrentValue;
            var outputJson = (string?)DbContext.Entry(entity).Property("SerializedOutputs").CurrentValue;
            var propertiesJson = (string?)DbContext.Entry(entity).Property("SerializedProperties").CurrentValue;
            var metadataJson = (string?)DbContext.Entry(entity).Property("SerializedMetadata").CurrentValue;
            var exceptionJson = (string?)DbContext.Entry(entity).Property("SerializedException").CurrentValue;

            entity.ActivityState = activityStateJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<IDictionary<string, object?>?>(activityStateJson);
            entity.Payload = payloadJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<IDictionary<string, object>?>(payloadJson);
            entity.Outputs = outputJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<IDictionary<string, object?>?>(outputJson);
            entity.Properties = propertiesJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<IDictionary<string, object?>?>(propertiesJson);
            entity.Metadata = metadataJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<IDictionary<string, object?>?>(metadataJson);
            entity.Exception = exceptionJson.IsNullOrEmpty() ? null : payloadSerializer.Deserialize<ExceptionState?>(exceptionJson);
        }
        catch (Exception exp)
        {
            logger.LogError(exp, "无法反序列化执行记录：{Id}", entity.Id);
        }

        return ValueTask.CompletedTask;
    }
}
