using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Domain.Data;

public interface ITriggerRepository : IRepository<TriggerEntity, long>
{
    Task<IEnumerable<TriggerEntity>> FindTriggersAsync(string activityTypeName, object stimulus, CancellationToken cancellationToken = default);

    Task<IEnumerable<TriggerEntity>> FindTriggersAsync(string stimulusHash, CancellationToken cancellationToken = default);
}
