namespace Cike.EntityFrameworkCore;

public class CikeWorkflowDbContenxt : CikeDbContext<CikeWorkflowDbContenxt>
{
    public DbSet<Folder> Folders { get; set; }

    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }

    public DbSet<WorkflowInstance> WorkflowInstances { get; set; }

    public DbSet<ActivityInstanceExecutionRecord> ActivityInstanceExecutionRecords { get; set; }

    public DbSet<BookmarkEntity> Bookmarks { get; set; }

    public DbSet<BookmarkQueueItem> BookmarkQueueItems { get; set; }

    public CikeWorkflowDbContenxt(DbContextOptions<CikeWorkflowDbContenxt> options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
