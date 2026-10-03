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
    // or runtime-generated types) and their cached property lookups can be reclaimed by the GC once
    // the type is no longer referenced elsewhere, instead of being pinned for the process lifetime.
    private static readonly ConditionalWeakTable<Type, ConcurrentDictionary<string, PropertyInfo?>> PropertyCache = [];

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Reflection is required here.")]
    public static object? GetValueByPath(this object obj, string path)
    {
        if (obj == null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var result = obj;

        foreach (var token in path.Split(PropertyOfOp))
        {
            // Resolve each token against the runtime type of the current value so that
            // polymorphic models (intermediate members declared as object/interface/base
            // types) still resolve derived properties. Caching per (runtime type, token)
            // keeps reflection lookups cheap without baking in declared-type semantics.
            var type = result.GetType();
            var propertiesByName = PropertyCache.GetValue(type, static _ => new ConcurrentDictionary<string, PropertyInfo?>());

            if (!propertiesByName.TryGetValue(token, out var property))
            {
                property = ResolveProperty(type, token);
                propertiesByName[token] = property;
            }

            result = property?.GetValue(result);

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
    private static PropertyInfo? ResolveProperty([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, string name)
        => type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
}
