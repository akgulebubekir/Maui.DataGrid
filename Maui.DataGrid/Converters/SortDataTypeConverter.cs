namespace Maui.DataGrid.Converters;

using System.ComponentModel;
using System.Globalization;

/// <summary>
/// Converts string to <see cref="SortData"/> enum.
/// </summary>
public sealed class SortDataTypeConverter : TypeConverter // This needs to be public or it will produce a MethodAccessException
{
    /// <inheritdoc/>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is int index || int.TryParse(value.ToString(), out index))
        {
            return (SortData)index;
        }

        var str = value.ToString();

        if (str != null)
        {
            var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 && int.TryParse(parts[0], out var columnIndex))
            {
                var order = parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase)
                    ? SortingOrder.Descendant
                    : SortingOrder.Ascendant;

                return new SortData(columnIndex, order);
            }
        }

        return base.ConvertFrom(context, culture, value);
    }
}
