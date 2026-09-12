using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.ActivityDescriptors.Internals;

internal class EmptyActivityProdiver : IActivityProvider, ISingletonDependency
{
    public async ValueTask<IEnumerable<ActivityDescriptor>> GetDescriptorsAsync(CancellationToken cancellationToken = default)
    {
        return await ValueTask.FromResult(new List<ActivityDescriptor>());
    }
}
