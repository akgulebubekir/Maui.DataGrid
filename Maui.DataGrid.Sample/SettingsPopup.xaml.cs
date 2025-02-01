namespace Maui.DataGrid.Sample;

using Maui.DataGrid.Sample.ViewModels;

/// <summary>
/// Codebehind for the SettingsPopup.
/// </summary>
internal partial class SettingsPopup
{
    private readonly MainViewModel _viewModel;

    internal SettingsPopup(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
    }

    public void OnClose(object sender, EventArgs e) => Close();

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

        var columnToRemove = _viewModel.Columns.FirstOrDefault(c => c.Title.Equals(columnTitle, StringComparison.OrdinalIgnoreCase));

        if (columnToRemove == null)
        {
            await Shell.Current.DisplayAlert("Column not found", "No column by that title", "Ok");
        }
        else
        {
            _ = _viewModel.Columns.Remove(columnToRemove);
        }
    }
}
