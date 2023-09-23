namespace Maui.DataGrid.Sample;

using Maui.DataGrid.Sample.ViewModels;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DataTablePage
{
    public DataTablePage()
    {
        InitializeComponent();
        BindingContext = new DataTableViewModel();
        _addColumnButton1.Clicked += OnAddColumn;
    }

    private void OnAddColumn(object sender, EventArgs e)
    {
        _dataGrid1.Columns.Add(new DataGridColumn() { Title = "Test", Width = new(100) });
    }
}
