namespace Maui.DataGrid.Sample;

using Maui.DataGrid.Sample.ViewModels;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();
    }

    private void OnAddColumn(object sender, EventArgs e)
    {
        _dataGrid1.Columns.Add(new DataGridColumn() { Title = "Test", Width = new(100) });
    }

    private async void OnRemoveColumn(object sender, EventArgs e)
    {
        var columnTitle = await Shell.Current.DisplayPromptAsync("Which column should be removed?", "Remove column");

        var teamColumn = _dataGrid1.Columns.FirstOrDefault(c => c.Title == columnTitle);

        if (teamColumn != null)
        {
            _ = _dataGrid1.Columns.Remove(teamColumn);
        }
        else
        {
            await Shell.Current.DisplayAlert("Column not found", "No column by that title", "Ok");
        }
    }
}
