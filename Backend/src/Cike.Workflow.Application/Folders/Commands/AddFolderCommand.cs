namespace Cike.Workflow.Application.Folders.Commands;

public record AddFolderCommand(AddFolderDto Dto) : Command
{
    public long Id { get; set; }
}
