using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.StorageDrivers;

public interface IStorageDriverManager
{
    IStorageDriver? Find(string type);
}
