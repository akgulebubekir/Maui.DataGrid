namespace Maui.DataGrid;

using System;
using System.Globalization;

internal class BoolToSelectionModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool selectionEnabled && selectionEnabled)
        {
            return SelectionMode.Single;
        }

        return SelectionMode.None;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
