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
        _addColumnButton1.Clicked += OnAddColumn;
    }

    private void OnAddColumn(object sender, EventArgs e)
    {
        _dataGrid1.Columns.Add(new DataGridColumn() { Title = "Test", Width = new(100) });
    }
}

internal class StreakToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Streak s)
        {
            return s.Result == Result.Won
                ? Colors.Green.AddLuminosity(s.NumStreak / 30F)
                : Colors.Red.AddLuminosity(-s.NumStreak / 30F);
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
