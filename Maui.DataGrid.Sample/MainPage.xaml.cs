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

    private async void OnAddColumn(object sender, EventArgs e)
    {
        var newColumnTitle = await Shell.Current.DisplayPromptAsync("Add column", "What is the name of the new column?");

        if (string.IsNullOrEmpty(newColumnTitle))
        {
            await Shell.Current.DisplayAlert("Title required", "A title is required in order to add a column.", "Ok");
        }
        else
        {
            _dataGrid1.Columns.Add(new DataGridColumn() { Title = newColumnTitle, Width = new(100) });
        }
    }

    private async void OnRemoveColumn(object sender, EventArgs e)
    {
        var columnTitle = await Shell.Current.DisplayPromptAsync("Remove column", "Which column should be removed?");

        var columnToRemove = _dataGrid1.Columns.FirstOrDefault(c => c.Title == columnTitle);

        if (columnToRemove == null)
        {
            await Shell.Current.DisplayAlert("Column not found", "No column by that title", "Ok");
        }
        else
        {
            _ = _dataGrid1.Columns.Remove(columnToRemove);
        }
    }
}
