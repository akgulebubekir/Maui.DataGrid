using System.Globalization;

namespace Maui.DataGrid;

/// <summary>
/// Sets ColumnWidth to either the parameter value or zero, depending on what boolean value it is bound to
/// </summary>
public class ConditionalWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
        {
            return isVisible ? parameter : new GridLength(0);
        }

        throw new ArgumentException($"{nameof(ConditionalWidthConverter)} only works with boolean values");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}