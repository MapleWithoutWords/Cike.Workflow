using Cike.Core.Hashers;
using Cike.Workflow.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Helpers
{
    public interface IStimulusHasher
    {
        /// <summary>
        /// Produces a hash from the specified activity type name, payload and activity instance ID.
        /// </summary>
        string Hash(string stimulusName, object? payload = null, long? activityInstanceId = null);
    }
}

public class StimulusHasher(IHasher hasher) : IStimulusHasher, ISingletonDependency
{
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string Hash(string stimulusName, object? payload = null, long? activityInstanceId = null)
    {
        return hasher.Hash(new object?[] { stimulusName, payload, activityInstanceId }, _serializerOptions);
    }
}
