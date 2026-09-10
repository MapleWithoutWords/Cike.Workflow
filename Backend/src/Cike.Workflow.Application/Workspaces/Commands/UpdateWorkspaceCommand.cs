namespace Cike.Workflow.Application.Workspaces.Commands;

public record UpdateWorkspaceCommand(long Id, UpdateWorkspaceDto Dto) : Command;
