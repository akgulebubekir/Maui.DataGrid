namespace Maui.DataGrid.Sample;

using Maui.DataGrid.Sample.ViewModels;

#pragma warning disable CA1812 // CA1812: MainPage is instantiated in XAML, so it appears unused to static analysis tools.

/// <summary>
/// Codebehind for the MainPage.
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
internal sealed partial class MainPage
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
