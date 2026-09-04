using Cike.Core.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Tests;

[DependsOn([
    typeof(CikeWorkflowCoreModule)
    ])]
internal class CikeWorkflowCoreTestModule : CikeModule
{
}
