namespace Maui.DataGrid.Sample.ViewModels;

using System.Collections.ObjectModel;
using Models;
using Utils;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        Teams = DummyDataProvider.GetTeams();
        TeamColumnVisible = true;
        WonColumnVisible = true;
        HeaderBordersVisible = true;
        PaginationEnabled = true;
        TeamColumnWidth = 70;
        SelectionMode = SelectionMode.Single;
        PageSize = 6;

        Commands.Add("CompleteEdit", new Command(CmdCompleteEdit));
        Commands.Add("Edit", new Command<Team>(CmdEdit));
        Commands.Add("Refresh", new Command(CmdRefresh));
    }

    public IEnumerable<SelectionMode> SelectionModes => Enum.GetValues<SelectionMode>().Cast<SelectionMode>();

    public ObservableCollection<DataGridColumn> Columns { get; set; }

    public Team TeamToEdit
    {
        get => GetValue<Team>();
        set => SetValue(value);
    }

    public List<Team> Teams
    {
        get => GetValue<List<Team>>();
        set => SetValue(value);
    }

    public bool HeaderBordersVisible
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    public bool TeamColumnVisible
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    public bool WonColumnVisible
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    public ushort TeamColumnWidth
    {
        get => GetValue<ushort>();
        set => SetValue(value);
    }

    public int PageSize
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    public bool PaginationEnabled
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    public SelectionMode SelectionMode
    {
        get => GetValue<SelectionMode>();
        set => SetValue(value);
    }

    public Team SelectedTeam
    {
        get => GetValue<Team>();
        set => SetValue(value);
    }

    public bool IsRefreshing
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    private void CmdCompleteEdit()
    {
        TeamToEdit = null;
    }

    private void CmdEdit(Team teamToEdit)
    {
        ArgumentNullException.ThrowIfNull(teamToEdit);

        TeamToEdit = teamToEdit;
    }

    private async void CmdRefresh()
    {
        IsRefreshing = true;
        // wait 3 secs for demo
        await Task.Delay(3000);
        IsRefreshing = false;
    }
}
