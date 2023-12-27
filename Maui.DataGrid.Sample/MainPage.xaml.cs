namespace Maui.DataGrid.Sample;

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

    private void OnRemoveTeamColumn(object sender, EventArgs e)
    {
        var teamColumn = _dataGrid1.Columns.FirstOrDefault(c => c.Title == "Team");

        if (teamColumn != null)
        {
            _ = _dataGrid1.Columns.Remove(teamColumn);
        }
    }
}
