namespace Cike.Workflow.EntityFrameworkCore.Tests.Repositories;

/// <summary>
/// DI 解析：6 个自定义仓储以约定注册覆盖同实体默认仓储；无附加行为的 BookmarkQueueItem 仍走框架默认仓储。
/// </summary>
public class RepositoryResolutionTest : RepositoryTestBase
{
    [Test]
    public void Resolve_CustomRepository_CoversDefaultRegistration()
    {
        using var scope = CreateScope();

        Assert.That(scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>(), Is.InstanceOf<WorkspaceRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IFolderRepository>(), Is.InstanceOf<FolderRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IWorkflowDefinitionRepository>(), Is.InstanceOf<WorkflowDefinitionRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>(), Is.InstanceOf<WorkflowInstanceRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IBookmarkRepository>(), Is.InstanceOf<BookmarkRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IActivityInstanceExecutionRecordRepository>(), Is.InstanceOf<ActivityInstanceExecutionRecordRepository>());

        Assert.That(scope.ServiceProvider.GetRequiredService<IRepository<Workspace, long>>(), Is.InstanceOf<WorkspaceRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IRepository<Folder, long>>(), Is.InstanceOf<FolderRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IRepository<WorkflowDefinition, long>>(), Is.InstanceOf<WorkflowDefinitionRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IRepository<WorkflowInstance, long>>(), Is.InstanceOf<WorkflowInstanceRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IRepository<BookmarkEntity, long>>(), Is.InstanceOf<BookmarkRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IRepository<ActivityInstanceExecutionRecord, long>>(), Is.InstanceOf<ActivityInstanceExecutionRecordRepository>());

        Assert.That(scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Workspace, long>>(), Is.InstanceOf<WorkspaceRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Folder, long>>(), Is.InstanceOf<FolderRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<WorkflowDefinition, long>>(), Is.InstanceOf<WorkflowDefinitionRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<WorkflowInstance, long>>(), Is.InstanceOf<WorkflowInstanceRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<BookmarkEntity, long>>(), Is.InstanceOf<BookmarkRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<ActivityInstanceExecutionRecord, long>>(), Is.InstanceOf<ActivityInstanceExecutionRecordRepository>());
    }

    [Test]
    public void Resolve_EntityWithoutCustomRepository_UsesDefault()
    {
        using var scope = CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<BookmarkQueueItem, long>>();

        Assert.That(repository, Is.InstanceOf<EfCoreRepository<CikeWorkflowDbContext, BookmarkQueueItem, long>>());
    }
}
