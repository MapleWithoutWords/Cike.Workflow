using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Application.Contracts.Workspaces;

public class UpdateWorkspaceDto
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
}
