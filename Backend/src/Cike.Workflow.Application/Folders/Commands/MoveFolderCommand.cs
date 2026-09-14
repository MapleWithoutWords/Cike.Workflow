namespace Cike.Workflow.Application.Folders.Commands;

/// <summary>
/// 移动目录到同工作空间的其他位置（ParentId 为 0 表示移到根目录）：
/// 子目录与其中的工作流经父子链自然跟随，只需更新本节点的 ParentId。
/// </summary>
public record MoveFolderCommand(long Id, long ParentId) : Command;
