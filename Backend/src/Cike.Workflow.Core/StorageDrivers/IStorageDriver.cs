using Cike.Workflow.Core.StorageDrivers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.StorageDrivers;

public interface IStorageDriver
{
    ValueTask WriteAsync(string id, object value, StorageDriverContext context);

    ValueTask<object?> ReadAsync(string id, StorageDriverContext context);

    ValueTask DeleteAsync(string id, StorageDriverContext context);
}
