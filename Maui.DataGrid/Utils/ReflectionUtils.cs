namespace Maui.DataGrid.Utils;

using System.ComponentModel;

internal static class ReflectionUtils
{
    private const char PropertyOfOp = '.';

    public static object? GetValueByPath(object obj, string path)
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
        var propertyDescriptor = TypeDescriptor.GetProperties(obj)[propertyName];

        if (propertyDescriptor is null)
        {
            return null;
        }

        return propertyDescriptor.GetValue(obj);
    }
}
