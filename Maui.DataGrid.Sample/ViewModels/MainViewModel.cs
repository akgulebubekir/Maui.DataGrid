namespace Maui.DataGrid.Sample.ViewModels;

using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Maui.DataGrid.Sample.Models;
using Maui.DataGrid.Sample.Utils;

public class MainViewModel : INotifyPropertyChanged
{
    private List<Team> _teams;
    private Team _selectedItem;
    private bool _isRefreshing;
    private bool _teamColumnVisible = true;
    private bool _wonColumnVisible = true;
    private bool _headerBordersVisible = true;
    private bool _paginationEnabled = true;
    private ushort _teamColumnWidth = 70;

    public MainViewModel()
    {
        Teams = DummyDataProvider.GetTeams();
        RefreshCommand = new Command(CmdRefresh);
    }

    public List<Team> Teams
    {
        get => _teams;
        set
        {
            _teams = value;
            OnPropertyChanged(nameof(Teams));
        }
    }

    public bool HeaderBordersVisible
    {
        get => _headerBordersVisible;
        set
        {
            _headerBordersVisible = value;
            OnPropertyChanged(nameof(HeaderBordersVisible));
        }
    }

    public bool TeamColumnVisible
    {
        get => _teamColumnVisible;
        set
        {
            _teamColumnVisible = value;
            OnPropertyChanged(nameof(TeamColumnVisible));
        }
    }

    public bool WonColumnVisible
    {
        get => _wonColumnVisible;
        set
        {
            _wonColumnVisible = value;
            OnPropertyChanged(nameof(WonColumnVisible));
        }
    }

    public ushort TeamColumnWidth
    {
        get => _teamColumnWidth;
        set
        {
            _teamColumnWidth = value;
            OnPropertyChanged(nameof(TeamColumnWidth));
        }
    }

    public bool PaginationEnabled
    {
        get => _paginationEnabled;
        set
        {
            _paginationEnabled = value;
            OnPropertyChanged(nameof(PaginationEnabled));
        }
    }

    public Team SelectedTeam
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            Debug.WriteLine("Team Selected : " + value?.Name);
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged(nameof(IsRefreshing));
        }
    }

    public ICommand RefreshCommand { get; set; }

    private async void CmdRefresh()
    {
        IsRefreshing = true;
        // wait 3 secs for demo
        await Task.Delay(3000);
        IsRefreshing = false;
    }

    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    #endregion INotifyPropertyChanged implementation
}
