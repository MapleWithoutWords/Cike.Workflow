using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Application.Contracts.Folders;

public class AddFolderDto
{
    public long WorkspaceId { get; set; }

    public string Name { get; set; } = null!;

    public long ParentId { get; set; }
}
