using Cike.Workflow.Core.StorageDrivers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.StorageDrivers;

public interface IStorageDriverRegistry
{
    void Add(StorageDriverDescriptor descriptor);

    void AddRange(IEnumerable<StorageDriverDescriptor> descriptors);

    IEnumerable<StorageDriverDescriptor> ListAll();

    StorageDriverDescriptor? Find(Func<StorageDriverDescriptor, bool> predicate);

    StorageDriverDescriptor? Find(string type);
}
