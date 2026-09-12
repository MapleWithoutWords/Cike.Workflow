namespace Cike.Workflow.EntityFrameworkCore.Tests.Repositories;

/// <summary>
/// Bookmark 仓储：影子属性（SerializedPayload / SerializedMetadata）序列化 round-trip，
/// 含列表查询反序列化与 null 安全语义。
/// </summary>
public class BookmarkRepositoryTest : RepositoryTestBase
{
    private static BookmarkEntity MakeBookmark(string marker, object? payload = null, IDictionary<string, string>? metadata = null) => new()
    {
        Name = marker,
        Hash = $"hash-{marker}",
        WorkflowInstanceId = 1,
        ActivityInstanceId = 1,
        CorrelationId = $"corr-{marker}",
        Payload = payload,
        Metadata = metadata,
    };

    [Test]
    public async Task InsertAsync_WithPayloadAndMetadata_WritesJsonToShadowProperties()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBookmarkRepository>();
            var entity = MakeBookmark("insert",
                payload: new Dictionary<string, object> { ["amount"] = 1 },
                metadata: new Dictionary<string, string> { ["source"] = "test" });

            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using var verifyScope = CreateScope();
        var dbContext = verifyScope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
        var tracked = await dbContext.Set<BookmarkEntity>().FirstOrDefaultAsync(e => e.Id == id);

        var payloadJson = (string?)dbContext.Entry(tracked!).Property("SerializedPayload").CurrentValue;
        var metadataJson = (string?)dbContext.Entry(tracked!).Property("SerializedMetadata").CurrentValue;

        Assert.That(payloadJson, Does.Contain("amount"));
        Assert.That(metadataJson, Does.Contain("source"));
    }

    [Test]
    public async Task InsertAsync_WithNullPayload_WritesNullToShadowProperties()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBookmarkRepository>();
            var entity = MakeBookmark("null-payload");

            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using var verifyScope = CreateScope();
        var dbContext = verifyScope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
        var tracked = await dbContext.Set<BookmarkEntity>().FirstOrDefaultAsync(e => e.Id == id);

        Assert.That(dbContext.Entry(tracked!).Property("SerializedPayload").CurrentValue, Is.Null);
        Assert.That(dbContext.Entry(tracked!).Property("SerializedMetadata").CurrentValue, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_WithChangedPayload_WritesNewJsonToShadowProperty()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBookmarkRepository>();
            var entity = MakeBookmark("update", payload: new Dictionary<string, object> { ["step"] = "before" });
            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBookmarkRepository>();
            var entity = await repository.FindAsync(id);

            entity!.Payload = new Dictionary<string, object> { ["step"] = "after" };

            await repository.UpdateAsync(entity);
        }

        using var verifyScope = CreateScope();
        var dbContext = verifyScope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
        var tracked = await dbContext.Set<BookmarkEntity>().FirstOrDefaultAsync(e => e.Id == id);
        var payloadJson = (string?)dbContext.Entry(tracked!).Property("SerializedPayload").CurrentValue;

        // 迁移修复项：Update 路径同样执行序列化
        Assert.That(payloadJson, Does.Contain("after"));
    }

    [Test]
    public async Task FindAsync_WithPersistedPayload_RestoresPayloadAndMetadata()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBookmarkRepository>();
            var entity = MakeBookmark("restore",
                payload: new Dictionary<string, object> { ["amount"] = 9 },
                metadata: new Dictionary<string, string> { ["lang"] = "zh" });
            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        var restored = await WithScopeAsync(sp => sp.GetRequiredService<IBookmarkRepository>().FindAsync(id));

        Assert.That(restored, Is.Not.Null);
        Assert.That(restored!.Payload, Is.Not.Null);
        Assert.That(restored.Metadata, Is.Not.Null);
        Assert.That(restored.Metadata["lang"], Is.EqualTo("zh"));
    }

    [Test]
    public async Task GetListAsync_WithFilter_DeserializesPayloadAndMetadata()
    {
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBookmarkRepository>();
            var entity = MakeBookmark("list",
                payload: new Dictionary<string, object> { ["amount"] = 3 },
                metadata: new Dictionary<string, string> { ["key"] = "value" });
            await repository.InsertAsync(entity);
        }

        var items = await WithScopeAsync(sp => sp.GetRequiredService<IBookmarkRepository>()
            .GetListAsync(e => e.Name == "list"));

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].Payload, Is.Not.Null);
        Assert.That(items[0].Metadata, Is.Not.Null);
        Assert.That(items[0].Metadata!["key"], Is.EqualTo("value"));
    }

    [Test]
    public async Task GetListAsync_WithDefaultSorting_SortsByCreatedAtDesc()
    {
        await Host.SeedAsync<BookmarkEntity>(e => { e.Name = "sort-1"; e.Hash = "h1"; e.WorkflowInstanceId = 7; e.ActivityInstanceId = 1; e.CorrelationId = "c1"; e.CreatedAt = new DateTime(2024, 1, 1); });
        await Host.SeedAsync<BookmarkEntity>(e => { e.Name = "sort-2"; e.Hash = "h2"; e.WorkflowInstanceId = 7; e.ActivityInstanceId = 2; e.CorrelationId = "c2"; e.CreatedAt = new DateTime(2024, 1, 2); });
        await Host.SeedAsync<BookmarkEntity>(e => { e.Name = "sort-3"; e.Hash = "h3"; e.WorkflowInstanceId = 7; e.ActivityInstanceId = 3; e.CorrelationId = "c3"; e.CreatedAt = new DateTime(2024, 1, 3); });

        var items = await WithScopeAsync(sp => sp.GetRequiredService<IBookmarkRepository>()
            .GetListAsync(e => e.WorkflowInstanceId == 7));

        Assert.That(items.Select(e => e.Name), Is.EqualTo(new[] { "sort-3", "sort-2", "sort-1" }));
    }
}
