namespace Maui.DataGrid.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Maui.DataGrid.Collections;

internal static class ReflectionExtensions
{
    private const char PropertyOfOp = '.';

    private static readonly ConcurrentLRUCache<string, object?> ValueCache = new(DataGrid.DefaultCacheSize);
    private static readonly ConcurrentLRUCache<string, Type?> TypeCache = new(DataGrid.DefaultCacheSize);

    private static int _cacheSize = DataGrid.DefaultCacheSize;

    public static void SetCacheSize(int cacheSize)
    {
        _cacheSize = cacheSize;
        ReinitializeCaches();
    }

    public static object? GetValueByPath(this object obj, string path, bool useCaching, int cacheSize)
    {
        if (obj == null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (useCaching && cacheSize != _cacheSize)
        {
            SetCacheSize(cacheSize);
        }

        var cacheKey = useCaching ? $"{obj.GetHashCode()}_{obj.GetType().FullName}.{path}" : string.Empty;

        if (useCaching && ValueCache.TryGetValue(cacheKey, out var cachedValue))
        {
            return cachedValue;
        }

        var result = obj;

        foreach (var token in path.Split(PropertyOfOp))
        {
            var resultType = result.GetType().GetProperty(token, BindingFlags.Public | BindingFlags.Instance);

            result = resultType?.GetValue(result);

            if (result == null)
            {
                if (useCaching)
                {
                    return ValueCache.TryGetOrAdd(cacheKey, null);
                }

                return null;
            }
        }

        if (useCaching)
        {
            return ValueCache.TryGetOrAdd(cacheKey, result);
        }

        return result;
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public static Type? GetPropertyTypeByPath([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] this Type type, string path, bool useCaching, int cacheSize)
    {
        if (type == null)
        {
            return null;
        }

        if (path == "." || string.IsNullOrWhiteSpace(path))
        {
            return type;
        }

        if (useCaching && cacheSize != _cacheSize)
        {
            SetCacheSize(cacheSize);
        }

        var cacheKey = useCaching ? $"{type.GetHashCode()}_{type.FullName}.{path}" : string.Empty;

        if (useCaching && TypeCache.TryGetValue(cacheKey, out var cachedType))
        {
            return cachedType;
        }

        var resultType = type;

        foreach (var token in path.Split(PropertyOfOp))
        {
            var property = resultType.GetProperty(token, BindingFlags.Public | BindingFlags.Instance);

            resultType = property?.PropertyType;

            if (resultType == null)
            {
                if (useCaching)
                {
                    return TypeCache.TryGetOrAdd(cacheKey, null);
                }

                return null;
            }
        }

        if (useCaching)
        {
            return TypeCache.TryGetOrAdd(cacheKey, resultType);
        }

        return resultType;
    }

    private static void ReinitializeCaches()
    {
        ValueCache.SetCapacity(_cacheSize);
        TypeCache.SetCapacity(_cacheSize);
    }
}
