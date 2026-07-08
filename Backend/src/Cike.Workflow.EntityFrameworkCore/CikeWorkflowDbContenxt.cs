namespace Cike.EntityFrameworkCore;

public class CikeWorkflowDbContenxt : CikeDbContext<CikeWorkflowDbContenxt>
{
    public CikeWorkflowDbContenxt(DbContextOptions<CikeWorkflowDbContenxt> options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
