using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Application.Contracts.Folders
{
    public class FolderDetailDto : AuditedEntityDto<long>
    {
        public long WorkspaceId { get; set; }

        public string Name { get; set; } = null!;

        public long ParentId { get; set; }

        public List<FolderPathDto> Path { get; set; } = [];
    }
}
