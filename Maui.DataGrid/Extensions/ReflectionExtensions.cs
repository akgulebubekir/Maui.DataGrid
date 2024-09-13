namespace Maui.DataGrid.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Maui.DataGrid.Collections;

internal static class ReflectionExtensions
{
    private const int DefaultCacheSize = 100000;
    private const char PropertyOfOp = '.';

    private static readonly ConcurrentLRUCache<string, object?> ValueCache = new(DefaultCacheSize);
    private static readonly ConcurrentLRUCache<string, Type?> TypeCache = new(DefaultCacheSize);

    private static int _cacheSize = DefaultCacheSize;

    public static void SetCacheSize(int cacheSize)
    {
        _cacheSize = cacheSize;
        ReinitializeCaches();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("Calls Maui.DataGrid.Extensions.ReflectionExtensions.GetPropertyValue(Object, String)")]
    public static object? GetValueByPath(this object obj, string path)
    {
        if (obj == null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var cacheKey = $"{obj.GetHashCode()}_{obj.GetType().FullName}.{path}";
        if (ValueCache.TryGetValue(cacheKey, out var cachedValue))
        {
            return cachedValue;
        }

        var result = obj;

        foreach (var token in path.Split(PropertyOfOp))
        {
            var resultType = result?.GetType().GetProperty(token, BindingFlags.Public | BindingFlags.Instance);

            result = resultType?.GetValue(result);

            if (result == null)
            {
                return ValueCache.TryGetOrAdd(cacheKey, null);
            }
        }

        return ValueCache.TryGetOrAdd(cacheKey, result);
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("Calls Maui.DataGrid.Extensions.ReflectionExtensions.GetPropertyType(String)")]
    public static Type? GetPropertyTypeByPath([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] this Type type, string path)
    {
        if (type == null)
        {
            return null;
        }

        if (path == "." || string.IsNullOrWhiteSpace(path))
        {
            return type;
        }

        var cacheKey = $"{type.GetHashCode()}_{type.FullName}.{path}";

        if (TypeCache.TryGetValue(cacheKey, out var cachedType))
        {
            return cachedType;
        }

        var resultType = type;

        foreach (var token in path.Split(PropertyOfOp))
        {
            var property = resultType?.GetProperty(token, BindingFlags.Public | BindingFlags.Instance);

            resultType = property?.PropertyType;

            if (resultType == null)
            {
                return TypeCache.TryGetOrAdd(cacheKey, null);
            }
        }

        return TypeCache.TryGetOrAdd(cacheKey, resultType);
    }

    private static void ReinitializeCaches()
    {
        ValueCache.SetCapacity(_cacheSize);
        TypeCache.SetCapacity(_cacheSize);
    }
}
