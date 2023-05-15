namespace Maui.DataGrid.Utils;

using System.Globalization;
using System.Reflection;

internal static class ReflectionUtils
{
    private const char IndexBeginOp = '[';
    private const char IndexEndOp = ']';
    private const char PropertyOfOp = '.';

    public static object? GetValueByPath(object obj, string path)
    {
        if (obj is null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var tokens = path.Split(IndexBeginOp, PropertyOfOp);

        var result = obj;

        foreach (var token in tokens)
        {
            if (result is null)
            {
                break;
            }

            var type = result.GetType();

            //  Property
            result = token.Contains(IndexEndOp)
                ? GetIndexValue(type, obj, token)
                : GetPropertyValue(type, obj, token);
        }

        return result;
    }

    private static object? GetPropertyValue(Type type, object obj, string propertyName)
    {
        try
        {
            return type.GetRuntimeProperty(propertyName)?.GetValue(obj);
        }
        catch
        {
            return null;
        }
    }

    private static object? GetIndexValue(Type type, object obj, string index)
    {
        var indexOperator = type.GetRuntimeProperty("Item");
        if (indexOperator != null)
        {
            var trimmedIndex = index.Trim().TrimEnd(IndexEndOp).Trim();

            // Looking up suitable index operator
            foreach (var parameter in indexOperator.GetIndexParameters())
            {
                try
                {
                    var indexVal = Convert.ChangeType(trimmedIndex, parameter.ParameterType, CultureInfo.InvariantCulture);
                    return indexOperator.GetValue(obj, new[] { indexVal });
                }
                catch
                {
                }
            }
        }

        return null;
    }
}
