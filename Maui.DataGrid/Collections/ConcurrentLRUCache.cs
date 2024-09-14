namespace Maui.DataGrid.Collections;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

internal sealed class ConcurrentLRUCache<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private readonly object _lock = new();

    private readonly ConcurrentDictionary<TKey, TValue> _cache;
    private readonly ConcurrentDictionary<TKey, DateTime> _lruSet;
    private int _capacity;

    public ConcurrentLRUCache(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));
        }

        _cache = new ConcurrentDictionary<TKey, TValue>();
        _lruSet = new ConcurrentDictionary<TKey, DateTime>();

        _capacity = capacity;
    }

    public bool Contains(TKey key)
    {
        lock (_lock)
        {
            return _cache.ContainsKey(key);
        }
    }

    public TValue? Get(TKey key)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                _lruSet[key] = DateTime.UtcNow;
                return value;
            }

            return default;
        }
    }

    public TValue TryGetOrAdd(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var existingValue))
            {
                _lruSet[key] = DateTime.UtcNow;
                return existingValue;
            }

            if (_cache.Count >= _capacity)
            {
                RemoveLeastRecentlyUsed();
            }

            _cache[key] = value;
            _lruSet[key] = DateTime.UtcNow;
            return value;
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out value!))
            {
                _lruSet[key] = DateTime.UtcNow;
                return true;
            }

            value = default!;
            return false;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
            _lruSet.Clear();
        }
    }

    /// <summary>
    /// Set the capacity of the cache.
    /// </summary>
    /// <remarks>
    /// WARNING: Thread-safety is not guaranteed. This method should only be called when no other threads are accessing the cache.
    /// </remarks>
    /// <param name="newCapacity">The new capacity of the cache.</param>
    /// <exception cref="ArgumentException">Thrown when the new capacity is less than or equal to zero.</exception>
    public void SetCapacity(int newCapacity)
    {
        if (newCapacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than zero.", nameof(newCapacity));
        }

        lock (_lock)
        {
            _capacity = newCapacity;

            var oldestToNewest = _lruSet.OrderBy(kvp => kvp.Value);

            foreach (var kvp in oldestToNewest)
            {
                if (_cache.Count <= _capacity)
                {
                    return;
                }

                if (_lruSet.TryRemove(kvp.Key, out _))
                {
                    _cache.TryRemove(kvp.Key, out _);
                }
            }
        }
    }

    public void Dispose()
    {
        Clear();
    }

    private void RemoveLeastRecentlyUsed()
    {
        var leastRecentlyUsedKey = default(TKey);
        var oldestAccessTime = DateTime.MaxValue;

        foreach (var kvp in _lruSet)
        {
            if (kvp.Value < oldestAccessTime)
            {
                oldestAccessTime = kvp.Value;
                leastRecentlyUsedKey = kvp.Key;
            }
        }

        if (leastRecentlyUsedKey != null && _lruSet.TryRemove(leastRecentlyUsedKey, out _))
        {
            _cache.TryRemove(leastRecentlyUsedKey, out _);
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        lock (_lock)
        {
            return _cache.ToList().GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
