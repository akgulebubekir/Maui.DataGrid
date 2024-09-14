namespace Maui.DataGrid.Collections;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

internal sealed class ConcurrentLRUCache<TKey, TValue> : IDisposable, IDictionary<TKey, TValue>
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

    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => _cache.Keys;

    public ICollection<TValue> Values => _cache.Values;

    public int Count => _cache.Count;

    public TValue this[TKey key]
    {
        get => Get(key)!;
        set => TryGetOrAdd(key, value);
    }

    public bool Contains(TKey key) => _cache.ContainsKey(key);

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

    public void SetCapacity(int newCapacity)
    {
        if (newCapacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than zero.", nameof(newCapacity));
        }

        lock (_lock)
        {
            _capacity = newCapacity;

            var oldestToNewest = _lruSet.OrderBy(kvp => kvp.Value).ToList();

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

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _cache.ToList().GetEnumerator();
    }

    public void Add(TKey key, TValue value) => TryGetOrAdd(key, value);

    public bool ContainsKey(TKey key)
    {
        return Contains(key);
    }

    public bool Remove(TKey key)
    {
        lock (_lock)
        {
            if (_cache.TryRemove(key, out _))
            {
                _lruSet.TryRemove(key, out _);
                return true;
            }

            return false;
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ContainsKey(item.Key) && EqualityComparer<TValue>.Default.Equals(this[item.Key], item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_cache).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        lock (_lock)
        {
            if (Contains(item))
            {
                return Remove(item.Key);
            }

            return false;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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
}
