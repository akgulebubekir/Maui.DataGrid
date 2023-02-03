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
    private bool _winColumnVisible = true;
    private bool _headerBordersVisible = true;

    public MainViewModel()
    {
        this.Teams = DummyDataProvider.GetTeams();
        this.RefreshCommand = new Command(this.CmdRefresh);
    }

    public List<Team> Teams
    {
        get => this._teams;
        set
        {
            this._teams = value;
            this.OnPropertyChanged(nameof(this.Teams));
        }
    }

    public bool HeaderBordersVisible
    {
        get => this._headerBordersVisible;
        set
        {
            this._headerBordersVisible = value;
            this.OnPropertyChanged(nameof(this.HeaderBordersVisible));
        }
    }

    public bool TeamColumnVisible
    {
        get => this._teamColumnVisible;
        set
        {
            this._teamColumnVisible = value;
            this.OnPropertyChanged(nameof(this.TeamColumnVisible));
        }
    }

    public bool WinColumnVisible
    {
        get => this._winColumnVisible;
        set
        {
            this._winColumnVisible = value;
            this.OnPropertyChanged(nameof(this.WinColumnVisible));
        }
    }

    public Team SelectedTeam
    {
        get => this._selectedItem;
        set
        {
            this._selectedItem = value;
            Debug.WriteLine("Team Selected : " + value?.Name);
        }
    }

    public bool IsRefreshing
    {
        get => this._isRefreshing;
        set
        {
            this._isRefreshing = value;
            this.OnPropertyChanged(nameof(this.IsRefreshing));
        }
    }

    public ICommand RefreshCommand { get; set; }

    private async void CmdRefresh()
    {
        this.IsRefreshing = true;
        // wait 3 secs for demo
        await Task.Delay(3000);
        this.IsRefreshing = false;
    }

    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    #endregion INotifyPropertyChanged implementation
}
