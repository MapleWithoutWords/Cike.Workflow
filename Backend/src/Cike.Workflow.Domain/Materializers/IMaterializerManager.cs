using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Domain.Materializers;

public interface IMaterializerManager
{
    IWorkflowMaterializer? GetMaterializer(string name);

    bool IsMaterializerAvailable(string name);
}
