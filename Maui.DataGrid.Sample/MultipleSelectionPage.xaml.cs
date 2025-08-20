namespace Maui.DataGrid.Sample;

using Maui.DataGrid.Sample.ViewModels;

/// <summary>
/// Codebehind for the MainPage.
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
internal partial class MultipleSelectionPage
{
    public MultipleSelectionPage()
    {
        InitializeComponent();

        BindingContext = new MultipleSelectionViewModel
        {
            Columns = _dataGrid1.Columns,
        };
    }
}
