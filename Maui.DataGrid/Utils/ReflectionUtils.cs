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

        foreach (var token in tokens)
        {
            if (result is null)
            {
                break;
            }

            //  Property
            result = !token.Contains(IndexEndOp)
                ? GetPropertyValue(result, token)
                : GetIndexValue(result, token.Replace(IndexEndOp.ToString(), ""));
        }

        return result;
    }

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        try
        {
            return obj?.GetType().GetRuntimeProperty(propertyName)?.GetValue(obj);
        }
        catch
        {
            return null;
        }
    }

    private static object? GetIndexValue(object obj, string index)
    {
        var indexOperator = obj?.GetType().GetRuntimeProperty("Item");
        if (indexOperator != null)
        {
            // Looking up suitable index operator
            foreach (var parameter in indexOperator.GetIndexParameters())
            {
                try
                {
                    var indexVal = Convert.ChangeType(index, parameter.ParameterType, CultureInfo.InvariantCulture);
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
