using Cike.Caching;
using Cike.Caching.Options;
using System.Text.Json;

namespace Cike.Workflow.Realtime.Tests.Infrastructure;

/// <summary>
/// IMultilevelCacheClient 内存替身：以 Singleton 注册（模拟 Redis 的跨 Scope 可见性），
/// 使 WorkflowDefinitionCache 真实实现（键设计 / 索引维护 / latest 选取）可在无 Redis 环境参与集成测试。
/// 值经 JSON 序列化存取，模拟分布式层的 round-trip 语义；所有过载统一走同一内存字典，
/// 过期/TTL/PubSub 参数一律忽略（内存介质无过期概念）。
/// </summary>
public class InMemoryMultilevelCacheClient : IMultilevelCacheClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string?> _store = new();

    // 模拟默认 CacheKeyType.TypeName：类型前缀隔离，不同泛型类型同键不冲突
    private static string FormatKey<T>(string key) => $"{typeof(T).FullName}:{key}";

    private void Store<T>(string key, T? value)
        => _store[FormatKey<T>(key)] = JsonSerializer.Serialize(value, JsonOptions);

    private T? GetStored<T>(string key)
    {
        if (!_store.TryGetValue(FormatKey<T>(key), out var raw) || raw == null)
            return default;
        return JsonSerializer.Deserialize<T>(raw, JsonOptions);
    }

    private void DeleteKey<T>(string key) => _store.TryRemove(FormatKey<T>(key), out _);

    #region Get / GetAsync

    public T? Get<T>(string key, Action<MultilevelCacheOptions>? action = null)
        => GetStored<T>(key);

    public T? Get<T>(string key, Action<T?> valueChanged, Action<MultilevelCacheOptions>? action = null)
    {
        var value = GetStored<T>(key);
        valueChanged?.Invoke(value);
        return value;
    }

    public Task<T?> GetAsync<T>(string key, Action<MultilevelCacheOptions>? action = null)
        => Task.FromResult(GetStored<T>(key));

    public Task<T?> GetAsync<T>(string key, Action<T?> valueChanged, Action<MultilevelCacheOptions>? action = null)
    {
        var value = GetStored<T>(key);
        valueChanged?.Invoke(value);
        return Task.FromResult(value);
    }

    #endregion

    #region GetList / GetListAsync

    public IEnumerable<T?> GetList<T>(params string[] keys)
        => keys.Select(GetStored<T>).ToList();

    public IEnumerable<T?> GetList<T>(IEnumerable<string> keys, Action<MultilevelCacheOptions>? action = null)
        => keys.Select(GetStored<T>).ToList();

    public Task<IEnumerable<T?>> GetListAsync<T>(params string[] keys)
        => Task.FromResult<IEnumerable<T?>>(keys.Select(GetStored<T>).ToList());

    public Task<IEnumerable<T?>> GetListAsync<T>(IEnumerable<string> keys, Action<MultilevelCacheOptions>? action = null)
        => Task.FromResult<IEnumerable<T?>>(keys.Select(GetStored<T>).ToList());

    #endregion

    #region GetOrSet / GetOrSetAsync

    public T? GetOrSet<T>(string key, Func<CacheEntry<T>> getter, Action<CacheEntryOptions>? factoryOptions = null, Action<CacheOptions>? action = null)
    {
        var existing = GetStored<T>(key);
        if (existing is not null)
            return existing;
        var entry = getter();
        Store(key, entry.Value);
        return entry.Value;
    }

    public T? GetOrSet<T>(string key, CombinedCacheEntry<T> cacheEntry, Action<CacheOptions>? action = null)
    {
        var existing = GetStored<T>(key);
        if (existing is not null)
            return existing;
        var func = cacheEntry.DistributedCacheEntryFunc;
        if (func == null)
            return default;
        var value = func().Value;
        Store(key, value);
        return value;
    }

    public Task<T?> GetOrSetAsync<T>(string key, Func<CacheEntry<T>> getter, Action<CacheEntryOptions>? factoryOptions = null, Action<CacheOptions>? action = null)
        => Task.FromResult(GetOrSet<T>(key, getter, factoryOptions, action));

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<CacheEntry<T>>> getter, Action<CacheEntryOptions>? factoryOptions = null, Action<CacheOptions>? action = null)
    {
        var existing = GetStored<T>(key);
        if (existing is not null)
            return existing;
        var entry = await getter();
        Store(key, entry.Value);
        return entry.Value;
    }

    public Task<T?> GetOrSetAsync<T>(string key, CombinedCacheEntry<T> cacheEntry, Action<CacheOptions>? action = null)
        => Task.FromResult(GetOrSet<T>(key, cacheEntry, action));

    #endregion

    #region Refresh

    public void Refresh<T>(params string[] keys)
    {
        // 内存介质无过期概念，续期语义为空操作
    }

    public Task RefreshAsync<T>(params string[] keys)
        => Task.CompletedTask;

    public void Refresh<T>(IEnumerable<string> keys, Action<CacheOptions>? action = null)
    {
    }

    public Task RefreshAsync<T>(IEnumerable<string> keys, Action<CacheOptions>? action = null)
        => Task.CompletedTask;

    #endregion

    #region Set / SetAsync（全部过载统一进同一字典，过期参数忽略）

    public void Set<T>(string key, T value, DateTimeOffset? expireTo = null, Action<CacheOptions>? action = null)
        => Store(key, value);

    public void Set<T>(string key, T value, TimeSpan? expireIn = null, Action<CacheOptions>? action = null)
        => Store(key, value);

    public void Set<T>(string key, T value, CacheEntryOptions? entryOptions = null, Action<CacheOptions>? action = null)
        => Store(key, value);

    public void Set<T>(string key, T value, CacheEntryOptions? l1EntryOptions = null, CacheEntryOptions? l2EntryOptions = null, Action<CacheOptions>? action = null)
        => Store(key, value);

    public void Set<T>(string key, T value, CombinedCacheEntryOptions? entryOptions = null, Action<CacheOptions>? action = null)
        => Store(key, value);

    public Task SetAsync<T>(string key, T value, DateTimeOffset? expireTo = null, Action<CacheOptions>? action = null)
    {
        Store(key, value);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expireIn = null, Action<CacheOptions>? action = null)
    {
        Store(key, value);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, CacheEntryOptions? entryOptions = null, Action<CacheOptions>? action = null)
    {
        Store(key, value);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, CacheEntryOptions? l1EntryOptions = null, CacheEntryOptions? l2EntryOptions = null, Action<CacheOptions>? action = null)
    {
        Store(key, value);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, CombinedCacheEntryOptions? entryOptions = null, Action<CacheOptions>? action = null)
    {
        Store(key, value);
        return Task.CompletedTask;
    }

    #endregion

    #region SetList / SetListAsync

    public void SetList<T>(Dictionary<string, T?> dictionary, DateTimeOffset? expireTo = null, Action<CacheOptions>? action = null)
    {
        foreach (var (key, value) in dictionary) Store(key, value);
    }

    public void SetList<T>(Dictionary<string, T?> dictionary, TimeSpan? expireIn = null, Action<CacheOptions>? action = null)
    {
        foreach (var (key, value) in dictionary) Store(key, value);
    }

    public void SetList<T>(Dictionary<string, T?> dictionary, CacheEntryOptions? entryOptions = null, Action<CacheOptions>? action = null)
    {
        foreach (var (key, value) in dictionary) Store(key, value);
    }

    public void SetList<T>(Dictionary<string, T?> dictionary, CacheEntryOptions? l1EntryOptions = null, CacheEntryOptions? l2EntryOptions = null, Action<CacheOptions>? action = null)
    {
        foreach (var (key, value) in dictionary) Store(key, value);
    }

    public void SetList<T>(Dictionary<string, T?> dictionary, CombinedCacheEntryOptions? entryOptions = null, Action<CacheOptions>? action = null)
    {
        foreach (var (key, value) in dictionary) Store(key, value);
    }

    public Task SetListAsync<T>(Dictionary<string, T?> dictionary, DateTimeOffset? expireTo = null, Action<CacheOptions>? action = null)
    {
        SetList(dictionary, expireTo, action);
        return Task.CompletedTask;
    }

    public Task SetListAsync<T>(Dictionary<string, T?> dictionary, TimeSpan? expireIn = null, Action<CacheOptions>? action = null)
    {
        SetList(dictionary, expireIn, action);
        return Task.CompletedTask;
    }

    public Task SetListAsync<T>(Dictionary<string, T?> dictionary, CacheEntryOptions? entryOptions = null, Action<CacheOptions>? action = null)
    {
        SetList(dictionary, entryOptions, action);
        return Task.CompletedTask;
    }

    public Task SetListAsync<T>(Dictionary<string, T?> dictionary, CacheEntryOptions? l1EntryOptions = null, CacheEntryOptions? l2EntryOptions = null, Action<CacheOptions>? action = null)
    {
        SetList(dictionary, l1EntryOptions, l2EntryOptions, action);
        return Task.CompletedTask;
    }

    public Task SetListAsync<T>(Dictionary<string, T?> dictionary, CombinedCacheEntryOptions? entryOptions = null, Action<CacheOptions>? action = null)
    {
        SetList(dictionary, entryOptions, action);
        return Task.CompletedTask;
    }

    #endregion

    #region Remove / RemoveAsync

    public void Remove<T>(params string[] keys)
    {
        foreach (var key in keys) DeleteKey<T>(key);
    }

    public void Remove<T>(string key, Action<CacheOptions>? action = null)
        => DeleteKey<T>(key);

    public void Remove<T>(IEnumerable<string> keys, Action<CacheOptions>? action = null)
    {
        foreach (var key in keys) DeleteKey<T>(key);
    }

    public Task RemoveAsync<T>(params string[] keys)
    {
        foreach (var key in keys) DeleteKey<T>(key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync<T>(string key, Action<CacheOptions>? action = null)
    {
        DeleteKey<T>(key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync<T>(IEnumerable<string> keys, Action<CacheOptions>? action = null)
    {
        foreach (var key in keys) DeleteKey<T>(key);
        return Task.CompletedTask;
    }

    #endregion
}
