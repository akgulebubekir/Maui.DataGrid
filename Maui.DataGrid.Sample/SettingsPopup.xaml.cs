namespace Maui.DataGrid.Sample;

using Maui.DataGrid.Sample.ViewModels;

public partial class SettingsPopup
{
    private readonly MainViewModel _viewModel;

    public SettingsPopup(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
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
            _viewModel.Columns.Add(new DataGridColumn() { Title = newColumnTitle, PropertyName = newColumnTitle, Width = new(100) });
        }
    }

    private async void OnRemoveColumn(object sender, EventArgs e)
    {
        var columnTitle = await Shell.Current.DisplayPromptAsync("Remove column", "Which column should be removed?");

        var columnToRemove = _viewModel.Columns.FirstOrDefault(c => c.Title == columnTitle);

        if (columnToRemove == null)
        {
            await Shell.Current.DisplayAlert("Column not found", "No column by that title", "Ok");
        }
        else
        {
            _ = _viewModel.Columns.Remove(columnToRemove);
        }
    }

    public void OnClose(object sender, EventArgs e) => Close();
}
