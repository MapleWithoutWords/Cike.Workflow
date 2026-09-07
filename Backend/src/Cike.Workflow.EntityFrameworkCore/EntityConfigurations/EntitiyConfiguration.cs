using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cike.Workflow.EntityFrameworkCore.EntityConfigurations;

public class EntitiyConfiguration :
    IEntityTypeConfiguration<Folder>,
    IEntityTypeConfiguration<WorkflowDefinition>,
    IEntityTypeConfiguration<WorkflowInstance>,
    IEntityTypeConfiguration<ActivityInstanceExecutionRecord>,
    IEntityTypeConfiguration<BookmarkEntity>,
    IEntityTypeConfiguration<BookmarkQueueItem>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever().HasComment("主键");
        builder.Property(e => e.ParentId).ValueGeneratedNever().HasComment("父级目录ID");
        builder.Property(e => e.Name).HasMaxLength(128).HasComment("名称");

        builder.HasIndex(e => e.ParentId);
        builder.HasIndex(e => e.CreatedBy);
    }

    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.Ignore(x => x.Options);
        builder.Property<string>("SerializedOptions").HasColumnType("json");

        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.DefinitionId).HasMaxLength(128);
        builder.Property(e => e.Name).HasMaxLength(128).HasComment("名称");
        builder.Property(e => e.Description).HasMaxLength(512).HasComment("描述");
        builder.Property(e => e.MaterializerName).HasMaxLength(64).HasComment("描述");
        builder.Property(e => e.OriginalStringData);

        builder.Property(e => e.PublishedNote).HasMaxLength(512).HasComment("版本说明");
        builder.Property(e => e.PublishedBy).HasComment("发布人ID");
        builder.Property(e => e.PublishedAt).HasComment("发布时间").HasDefaultValue(DateTime.MinValue);

        builder.HasIndex(x => new { x.DefinitionId, x.Version, x.IsDeleted });
        builder.HasIndex(x => x.FolderId);
        builder.HasIndex(x => x.Version);
        builder.HasIndex(x => x.UsableAsActivity);
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsLatest);
        builder.HasIndex(x => x.IsPublished);
        builder.HasIndex(x => x.IsSystem);
    }

    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.DefinitionId).HasMaxLength(128);
        builder.Property(e => e.Name).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(e => e.CorrelationId).HasMaxLength(128).HasDefaultValue(string.Empty);

        builder.Ignore(x => x.WorkflowState);
        builder.Property<string>("SerializedWorkflowState").HasColumnType("json");
        builder.Property<string>("SerializedWorkflowStateCompressionAlgorithm").HasMaxLength(16);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(64);
        builder.HasIndex(x => new { x.TenantId, x.Status, x.Version });
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.IsExecuting });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.UpdatedBy);
        builder.HasIndex(x => x.FinishedAt);
    }

    public void Configure(EntityTypeBuilder<ActivityInstanceExecutionRecord> builder)
    {
        builder.Ignore(x => x.ActivityState);
        builder.Ignore(x => x.Exception);
        builder.Ignore(x => x.Payload);
        builder.Ignore(x => x.Outputs);
        builder.Ignore(x => x.Metadata);
        builder.Ignore(x => x.Properties);

        builder.Property<string>("SerializedActivityState").HasColumnType("json");
        builder.Property<string>("SerializedActivityStateCompressionAlgorithm").HasMaxLength(16);
        builder.Property<string>("SerializedException").HasColumnType("json");
        builder.Property<string>("SerializedPayload").HasColumnType("json");
        builder.Property<string>("SerializedOutputs").HasColumnType("json");
        builder.Property<string>("SerializedMetadata").HasColumnType("json");
        builder.Property<string>("SerializedProperties").HasColumnType("json");

        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.ActivityId).HasMaxLength(128);
        builder.Property(e => e.ActivityNodeId).HasMaxLength(128);
        builder.Property(e => e.ActivityType).HasMaxLength(128);
        builder.Property(e => e.ActivityName).HasMaxLength(128);
        builder.Property(e => e.SchedulingActivityId).HasMaxLength(128);
        builder.Property(e => e.TenantId).HasMaxLength(64);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(64);

        builder.HasIndex(x => x.WorkflowInstanceId);
        builder.HasIndex(x => x.ActivityId);
        builder.HasIndex(x => x.ActivityType);
        builder.HasIndex(x => x.ActivityTypeVersion);
        builder.HasIndex(x => new
        {
            x.ActivityType,
            x.ActivityTypeVersion
        });
        builder.HasIndex(x => x.ActivityName);
        builder.HasIndex(x => x.HasBookmarks);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.FinishedAt);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.SchedulingActivityId);
        builder.HasIndex(x => x.SchedulingWorkflowInstanceId);
    }

    public void Configure(EntityTypeBuilder<BookmarkEntity> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(256);
        builder.Property(e => e.Hash).HasMaxLength(256);

        builder.Ignore(x => x.Payload);
        builder.Ignore(x => x.Metadata);
        builder.Property<string>("SerializedPayload").HasColumnType("json");
        builder.Property<string>("SerializedMetadata").HasColumnType("json");

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Hash);
        builder.HasIndex(x => x.WorkflowInstanceId);
        builder.HasIndex(x => x.ActivityInstanceId);
        builder.HasIndex(x => new
        {
            x.Name,
            x.Hash
        });
        builder.HasIndex(x => new
        {
            x.Name,
            x.Hash,
            x.WorkflowInstanceId
        });
    }

    public void Configure(EntityTypeBuilder<BookmarkQueueItem> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Ignore(x => x.Options);
        builder.Property(x => x.SerializedOptions);
        builder.Property(e => e.CorrelationId).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(e => e.ActivityTypeName).HasMaxLength(256).HasDefaultValue(string.Empty);

        builder.HasIndex(x => x.StimulusHash);
        builder.HasIndex(x => x.WorkflowInstanceId);
        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => x.BookmarkId);
        builder.HasIndex(x => x.ActivityInstanceId);
        builder.HasIndex(x => x.ActivityTypeName);
        builder.HasIndex(x => x.CreatedAt);
    }
}
