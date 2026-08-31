using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.ActivityDescriptors;

public interface IActivityRegistryLookupService
{
    Task<ActivityDescriptor?> FindAsync(string type);

    Task<ActivityDescriptor?> FindAsync(string type, int version);

    Task<ActivityDescriptor?> FindAsync(Func<ActivityDescriptor, bool> predicate);

    IEnumerable<ActivityDescriptor> FindMany(Func<ActivityDescriptor, bool> predicate);
}
