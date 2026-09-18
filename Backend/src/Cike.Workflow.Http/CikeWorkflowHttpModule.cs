using Cike.Core.Modularity;
using Cike.Workflow.Core;

namespace Cike.Workflow.Http
{
    [DependsOn([
        typeof(CikeWorkflowCoreModule)
        ])]
    public class CikeWorkflowHttpModule : CikeModule
    {

    }
}
