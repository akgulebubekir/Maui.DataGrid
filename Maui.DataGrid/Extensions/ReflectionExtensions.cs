namespace Maui.DataGrid.Extensions;

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

internal static class ReflectionExtensions
{
    private const char PropertyOfOp = '.';

    // Keyed weakly by Type so that collectible types (e.g. from unloadable AssemblyLoadContexts
    // or runtime-generated types) and their cached property paths can be reclaimed by the GC once
    // the type is no longer referenced elsewhere, instead of being pinned for the process lifetime.
    private static readonly ConditionalWeakTable<Type, ConcurrentDictionary<string, PropertyInfo[]?>> PropertyPathCache = [];

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Reflection is required here.")]
    public static object? GetValueByPath(this object obj, string path)
    {
        if (obj == null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var type = obj.GetType();
        var propertiesByPath = PropertyPathCache.GetValue(type, static _ => new ConcurrentDictionary<string, PropertyInfo[]?>());

        if (!propertiesByPath.TryGetValue(path, out var properties))
        {
            properties = ResolvePropertyPath(type, path);
            propertiesByPath[path] = properties;
        }

        if (properties == null)
        {
            return null;
        }

        var result = obj;

        foreach (var property in properties)
        {
            result = property.GetValue(result);

            if (result == null)
            {
                return null;
            }
        }

        return result;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Reflection is needed here.")]
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

        var resultType = type;

        foreach (var token in path.Split(PropertyOfOp))
        {
            var property = resultType.GetProperty(token, BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                return null;
            }

            resultType = property.PropertyType;
        }

        return resultType;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Reflection is required here.")]
    private static PropertyInfo[]? ResolvePropertyPath([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, string path)
    {
        var tokens = path.Split(PropertyOfOp);
        var properties = new PropertyInfo[tokens.Length];
        var currentType = type;

        for (var i = 0; i < tokens.Length; i++)
        {
            var property = currentType.GetProperty(tokens[i], BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                return null;
            }

            properties[i] = property;
            currentType = property.PropertyType;
        }

        return properties;
    }
}
