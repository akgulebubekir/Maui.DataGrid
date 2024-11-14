namespace Maui.DataGrid.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

internal static class ReflectionExtensions
{
    private const char PropertyOfOp = '.';

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
            var resultType = result.GetType().GetProperty(token, BindingFlags.Public | BindingFlags.Instance);

            result = resultType?.GetValue(result);

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
}
