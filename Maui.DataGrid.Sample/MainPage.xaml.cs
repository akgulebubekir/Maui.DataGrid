namespace Maui.DataGrid.Sample;

using CommunityToolkit.Maui.Views;
using Maui.DataGrid.Sample.ViewModels;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new MainViewModel
        {
            Columns = _dataGrid1.Columns
        };
    }
}
