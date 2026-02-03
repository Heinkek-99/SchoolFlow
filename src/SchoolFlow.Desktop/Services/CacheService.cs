using System.Collections.Concurrent;
using SchoolFlow.Desktop.Services.Interfaces;

namespace SchoolFlow.Desktop.Services;

/// <summary>
/// Service de cache en mémoire avec expiration.
/// </summary>
public class CacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

    public async Task<T?> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
        {
            return (T)entry.Value!;
        }

        var value = await factory();
        Set(key, value, expiration);
        return value;
    }

    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
        {
            return (T)entry.Value!;
        }

        Remove(key);
        return default;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var entry = new CacheEntry(value, expiration ?? _defaultExpiration);
        _cache.AddOrUpdate(key, entry, (_, _) => entry);
    }

    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public void InvalidateByPrefix(string prefix)
    {
        var keysToRemove = _cache.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        foreach (var key in keysToRemove)
        {
            Remove(key);
        }
    }

    private class CacheEntry
    {
        public object? Value { get; }
        public DateTime ExpiresAt { get; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public CacheEntry(object? value, TimeSpan expiration)
        {
            Value = value;
            ExpiresAt = DateTime.UtcNow.Add(expiration);
        }
    }
}
