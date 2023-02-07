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

        var result = obj;
        var tokens = path?.Split(IndexBeginOp, PropertyOfOp);

        if (tokens is null)
        {
            return null;
        }

        foreach (var token in tokens)
        {
            if (result == null)
            {
                break;
            }

            //  Property
            result = !token.Contains(IndexEndOp.ToString())
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
        object? result = null;

        var indexOperator = obj?.GetType().GetRuntimeProperty("Item");
        if (indexOperator != null)
        {
            // Looking up suitable index operator
            foreach (var parameter in indexOperator.GetIndexParameters())
            {
                var isIndexOpWorked = true;
                try
                {
                    var indexVal = Convert.ChangeType(index, parameter.ParameterType, CultureInfo.InvariantCulture);
                    result = indexOperator.GetValue(obj, new[] { indexVal });
                }
                catch
                {
                    isIndexOpWorked = false;
                }

                // If the index operator worked, skip looking up others
                if (isIndexOpWorked)
                {
                    break;
                }
            }
        }

        return result;
    }
}
