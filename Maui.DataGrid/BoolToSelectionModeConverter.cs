namespace Maui.DataGrid;

using System.Globalization;

internal sealed class BoolToSelectionModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is true ?
            SelectionMode.Single : SelectionMode.None;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
