namespace Maui.DataGrid.Extensions;

using System;
using System.Collections.Concurrent;
using System.ComponentModel;

internal static class ReflectionExtensions
{
    private const char PropertyOfOp = '.';

    private static readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> PropertyTypeCache = new();

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

    public static Type? GetPropertyTypeByPath(this Type type, string path)
    {
        if (type == null || string.IsNullOrWhiteSpace(path))
        {
            return null;
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

    private static Type? GetPropertyType(this Type type, string propertyName)
    {
        var propertyDescriptor = GetPropertyDescriptor(type, propertyName);

        return propertyDescriptor?.PropertyType;
    }

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        var type = obj.GetType();

        var propertyDescriptor = GetPropertyDescriptor(type, propertyName);

        return propertyDescriptor?.GetValue(obj);
    }

    private static PropertyDescriptor? GetPropertyDescriptor(Type type, string propertyName)
    {
        if (!PropertyTypeCache.TryGetValue(type, out var properties))
        {
            properties = TypeDescriptor.GetProperties(type);
            PropertyTypeCache[type] = properties;
        }

        return properties.Find(propertyName, false);
    }
}
