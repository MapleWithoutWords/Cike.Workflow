namespace Cike.Workflow.Application.Workspaces.Commands;

public record AddWorkspaceCommand(AddWorkspaceDto Dto) : Command
{
    public long Id { get; set; }
}
