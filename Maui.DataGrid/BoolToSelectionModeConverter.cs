namespace Maui.DataGrid;

using System.Globalization;

internal sealed class BoolToSelectionModeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? SelectionMode.Single : SelectionMode.None;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SelectionMode selectionMode)
        {
            throw new InvalidDataException("Converter can only process SelectionMode enums");
        }

        return selectionMode == SelectionMode.Single;
    }
}
