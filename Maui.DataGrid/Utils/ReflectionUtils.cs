namespace Maui.DataGrid.Utils;

using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls.PlatformConfiguration;

internal static class ReflectionUtils
{
    private const char IndexBeginOp = '[';
    private const char IndexEndOp = ']';
    private const char PropertyOfOp = '.';

    public static object? GetValueByPath(object obj, string path)
    {
        if (obj is null)
        {
            return null;
        }

        var tokens = path?.Split(IndexBeginOp, PropertyOfOp);

        if (tokens is null)
        {
            return null;
        }

        var result = obj;

        var type = obj.GetType();

        foreach (var token in tokens)
        {
            if (result is null)
            {
                break;
            }

            //  Property
            result = !token.Contains(IndexEndOp)
                ? GetPropertyValue(type, obj, token)
                : GetIndexValue(type, obj, token);
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
            // Looking up suitable index operator
            foreach (var parameter in indexOperator.GetIndexParameters())
            {
                try
                {
                    var trimmedIndex = index.Replace(IndexEndOp.ToString(), "");
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
