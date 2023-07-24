namespace Maui.DataGrid;

using System.ComponentModel;
using System.Globalization;

/// <summary>
/// Converts string to <c>SortData</c> enum.
/// </summary>
public sealed class SortDataTypeConverter : TypeConverter // This needs to be public or it will produce a MethodAccessException
{
    /// <inheritdoc/>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (int.TryParse(value.ToString(), out var index))
        {
            return (SortData)index;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
