namespace Maui.DataGrid.Extensions;

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class ReflectionExtensions
{
    private const char PropertyOfOp = '.';

    private static readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> PropertyTypeCache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("Calls Maui.DataGrid.Extensions.ReflectionExtensions.GetPropertyValue(Object, String)")]
    public static object? GetValueByPath(this object obj, string path)
    {
        if (obj == null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        object? result;

        if (path.Contains(PropertyOfOp, StringComparison.Ordinal))
        {
            var tokens = path.Split(PropertyOfOp);

            result = obj;

            foreach (var token in tokens)
            {
                result = GetPropertyValue(result, token);

                if (result == null)
                {
                    break;
                }
            }
        }
        else
        {
            result = GetPropertyValue(obj, path);
        }

        return result;
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("Calls Maui.DataGrid.Extensions.ReflectionExtensions.GetPropertyType(String)")]
    public static Type? GetPropertyTypeByPath([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] this Type type, string path)
    {
        if (type == null)
        {
            return null;
        }

        if (path == "." || string.IsNullOrWhiteSpace(path))
        {
            return type;
        }

        Type? resultType;

        if (path.Contains(PropertyOfOp, StringComparison.Ordinal))
        {
            var tokens = path.Split(PropertyOfOp);

            resultType = type;

            foreach (var token in tokens)
            {
                resultType = resultType.GetPropertyType(token);

                if (resultType == null)
                {
                    break;
                }
            }
        }
        else
        {
            resultType = type.GetPropertyType(path);
        }

        return resultType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("Calls Maui.DataGrid.Extensions.ReflectionExtensions.GetPropertyDescriptor(Type, String)")]
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private static Type? GetPropertyType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] this Type type,
        string propertyName)
    {
        var propertyDescriptor = GetPropertyDescriptor(type, propertyName);

        return propertyDescriptor?.PropertyType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("Calls Maui.DataGrid.Extensions.ReflectionExtensions.GetPropertyDescriptor(Type, String)")]
    private static object? GetPropertyValue(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] object obj,
        string propertyName)
    {
        var type = obj.GetType();

        var propertyDescriptor = GetPropertyDescriptor(type, propertyName);

        return propertyDescriptor?.GetValue(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("Calls System.ComponentModel.TypeDescriptor.GetProperties(Type)")]
    private static PropertyDescriptor? GetPropertyDescriptor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        string propertyName)
    {
        var properties = PropertyTypeCache.GetOrAdd(type, TypeDescriptor.GetProperties);

        return properties.Find(propertyName, false);
    }
}
