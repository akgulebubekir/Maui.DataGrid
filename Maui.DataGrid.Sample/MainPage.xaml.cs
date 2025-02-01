namespace Maui.DataGrid.Sample;

using Maui.DataGrid.Sample.ViewModels;

/// <summary>
/// Codebehind for the MainPage.
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
internal partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new MainViewModel
        {
            Columns = _dataGrid1.Columns,
        };
    }
}
