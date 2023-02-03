namespace Maui.DataGrid.Sample;

using System.Globalization;
using Maui.DataGrid.Sample.Models;
using Maui.DataGrid.Sample.ViewModels;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();
    }
}

internal class StreakToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Streak s)
        {
            return s.Result == Result.Win
                ? Colors.Green.AddLuminosity(s.NumStreak / 30F)
                : Colors.Red.AddLuminosity(-s.NumStreak / 30F);
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
