namespace Maui.DataGrid.Sample.Converters;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Maui.DataGrid.Sample.Models;

[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via XAML")]
internal sealed class StreakToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Streak s)
        {
            return s.Result == GameResult.Won
                ? Colors.Green.AddLuminosity(s.NumStreak / 30F)
                : Colors.Red.AddLuminosity(-s.NumStreak / 30F);
        }

        return Colors.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
