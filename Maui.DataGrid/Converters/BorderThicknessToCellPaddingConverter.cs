namespace Maui.DataGrid.Converters;

using System.Globalization;

public class BorderThicknessToCellPaddingConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is Thickness thickness)
        {
            return new Thickness(thickness.Left / 2, thickness.Top / 2, thickness.Right / 2, thickness.Bottom / 2);
        }

        return new Thickness(0);
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}
