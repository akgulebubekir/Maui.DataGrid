using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Maui.DataGrid.Sample.Models;
using Maui.DataGrid.Sample.Utils;

namespace Maui.DataGrid.Sample.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private List<Team> _teams;
    private Team _selectedItem;
    private bool _isRefreshing;
    private bool _teamColumnVisible = true;
    private bool _winColumnVisible = true;
    private bool _headerBordersVisible = true;

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

    public bool WinColumnVisible
    {
        get => _winColumnVisible;
        set
        {
            _winColumnVisible = value;
            OnPropertyChanged(nameof(WinColumnVisible));
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

    private void OnPropertyChanged(string property)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    #endregion
}