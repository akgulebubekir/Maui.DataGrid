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