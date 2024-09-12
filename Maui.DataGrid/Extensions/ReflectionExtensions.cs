namespace Maui.DataGrid.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

internal static class ReflectionExtensions
{
    private const char PropertyOfOp = '.';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("Calls Maui.DataGrid.Extensions.ReflectionExtensions.GetPropertyValue(Object, String)")]
    public static object? GetValueByPath(this object obj, string path)
    {
        if (obj == null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var result = obj;

        foreach (var token in path.Split(PropertyOfOp))
        {
            var resultType = result?.GetType().GetProperty(token, BindingFlags.Public | BindingFlags.Instance);

            result = resultType?.GetValue(result);

            if (result == null)
            {
                return null;
            }
        }

        return result;
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

        var resultType = type;

        foreach (var token in path.Split(PropertyOfOp))
        {
            var property = resultType?.GetProperty(token, BindingFlags.Public | BindingFlags.Instance);

            resultType = property?.PropertyType;

            if (resultType == null)
            {
                return null;
            }
        }

        return resultType;
    }
}
