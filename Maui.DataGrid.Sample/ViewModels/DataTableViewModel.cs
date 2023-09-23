namespace Maui.DataGrid.Sample.ViewModels;

using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Maui.DataGrid.Sample.Utils;

public class DataTableViewModel : INotifyPropertyChanged
{
    private DataView _teams;
    private DataRowView _selectedItem;
    private bool _isRefreshing;
    private bool _teamColumnVisible = true;
    private bool _wonColumnVisible = true;
    private bool _headerBordersVisible = true;
    private bool _paginationEnabled = true;
    private ushort _teamColumnWidth = 70;

    readonly MethodInfo _method = typeof(DataRowView).GetMethod("RaisePropertyChangedEvent", BindingFlags.Instance | BindingFlags.NonPublic);

    public DataTableViewModel()
    {
        Teams = DummyDataProvider.GetDataTableTeams();
        RefreshCommand = new Command(CmdRefresh);
        ChangeWonCommand = new Command(ChangeWon);
    }

    public DataView Teams
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

    public DataRowView SelectedTeam
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            Debug.WriteLine("Team Selected : " + value?.Row);
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

    public ICommand ChangeWonCommand { get; set; }

    private async void CmdRefresh()
    {
        IsRefreshing = true;
        // wait 3 secs for demo
        await Task.Delay(3000);
        IsRefreshing = false;
    }

    private void ChangeWon()
    {
        if (SelectedTeam != null)
        {
            SelectedTeam["Won"] = new Random().Next(10, 100);
            _method.Invoke(SelectedTeam, new object[] { "Item[Won]" });
        }
    }

    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    #endregion INotifyPropertyChanged implementation
}
