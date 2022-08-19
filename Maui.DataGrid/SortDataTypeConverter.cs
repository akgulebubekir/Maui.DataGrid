using System.ComponentModel;
using System.Globalization;

namespace Maui.DataGrid;

public class SortDataTypeConverter : TypeConverter
{
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (int.TryParse(value?.ToString(), out var index))
        {
            return (SortData)index;
        }

        return base.ConvertFrom(context, culture, value);
    }
}