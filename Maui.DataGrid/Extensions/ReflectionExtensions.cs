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

        if (path.Contains(PropertyOfOp))
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

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        var properties = PropertyTypeCache.GetOrAdd(obj.GetType(), _ => TypeDescriptor.GetProperties(obj));

        var propertyDescriptor = properties.Find(propertyName, false);

        return propertyDescriptor?.GetValue(obj);
    }
}
