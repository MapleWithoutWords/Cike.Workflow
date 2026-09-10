namespace Cike.Workflow.Application.Folders.Commands;

public record UpdateFolderCommand(long Id, UpdateFolderDto Dto) : Command;
