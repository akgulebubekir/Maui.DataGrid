using System.Globalization;
using Maui.DataGrid.Sample.ViewModels;

namespace Maui.DataGrid.Sample;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();

        var dg = new DataGrid();
        var dgc = new DataGridColumn();
    }
}

internal class StreakToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return Colors.Transparent;
        }

        var val = value.ToString();

        var tokens = val.Split(' ');
        if (tokens.Length != 2)
        {
            throw new ArgumentException("incorrect streak format");
        }

        if (!int.TryParse(tokens[1], out var numStreak))
        {
            throw new ArgumentException("incorrect streak format");
        }

        return tokens.First() == "W"
            ? Colors.Green.AddLuminosity(numStreak / 30F)
            : Colors.Red.AddLuminosity(-numStreak / 30F);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}