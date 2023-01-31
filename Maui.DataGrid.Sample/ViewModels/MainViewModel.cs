namespace Maui.DataGrid.Sample.ViewModels;

using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Maui.DataGrid.Sample.Models;
using Maui.DataGrid.Sample.Utils;

public class MainViewModel : INotifyPropertyChanged
{
    private List<Team> teams;
    private Team selectedItem;
    private bool isRefreshing;
    private bool teamColumnVisible = true;
    private bool winColumnVisible = true;
    private bool headerBordersVisible = true;

    public MainViewModel()
    {
        this.Teams = DummyDataProvider.GetTeams();
        this.RefreshCommand = new Command(this.CmdRefresh);
    }

    public List<Team> Teams
    {
        get => this.teams;
        set
        {
            this.teams = value;
            this.OnPropertyChanged(nameof(this.Teams));
        }
    }

    public bool HeaderBordersVisible
    {
        get => this.headerBordersVisible;
        set
        {
            this.headerBordersVisible = value;
            this.OnPropertyChanged(nameof(this.HeaderBordersVisible));
        }
    }

    public bool TeamColumnVisible
    {
        get => this.teamColumnVisible;
        set
        {
            this.teamColumnVisible = value;
            this.OnPropertyChanged(nameof(this.TeamColumnVisible));
        }
    }

    public bool WinColumnVisible
    {
        get => this.winColumnVisible;
        set
        {
            this.winColumnVisible = value;
            this.OnPropertyChanged(nameof(this.WinColumnVisible));
        }
    }

    public Team SelectedTeam
    {
        get => this.selectedItem;
        set
        {
            this.selectedItem = value;
            Debug.WriteLine("Team Selected : " + value?.Name);
        }
    }

    public bool IsRefreshing
    {
        get => this.isRefreshing;
        set
        {
            this.isRefreshing = value;
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
