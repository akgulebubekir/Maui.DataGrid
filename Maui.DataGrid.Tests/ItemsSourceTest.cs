namespace Maui.DataGrid.Tests;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Maui.DataGrid.Tests.Models;
using Maui.DataGrid.Tests.TestUtils;
using Xunit;

public class ItemsSourceTest
{
    private readonly List<Team> _teams = Utils.DummyDataProvider.GetTeams();
    private readonly Team _dummyTeam = new()
    {
        Name = "Not Exists",
        Conf = string.Empty,
        Div = string.Empty,
        Home = string.Empty,
        Last10 = string.Empty,
        Logo = string.Empty,
        Road = string.Empty,
        Streak = new Streak { NumStreak = 3, Result = GameResult.Lost },
    };

    [Fact]
    public void BindsItemSource()
    {
        var dataGrid = new DataGrid();
        dataGrid.CheckPropertyBindingWorks(DataGrid.ItemsSourceProperty, _teams, null);
    }

    [Fact]
    public void BindsSelectedItem()
    {
        var datagrid = new DataGrid { ItemsSource = _teams };
        datagrid.CheckPropertyBindingWorks(DataGrid.SelectedItemProperty, _teams[2], _teams[3]);
    }

    [Fact]
    public async Task SelectNonExistingItemNotPossible()
    {
        var viewModel = new SingleVM<Team>();
        var datagrid = new DataGrid { ItemsSource = _teams };

        datagrid.SetBinding(DataGrid.SelectedItemProperty, new Binding("Item", source: viewModel));

        viewModel.Item = _teams[0];
        Assert.Equal(_teams[0], await datagrid.GetValueSafe(DataGrid.SelectedItemProperty));

        viewModel.Item = _dummyTeam;
        Assert.Null(await datagrid.GetValueSafe(DataGrid.SelectedItemProperty));
    }

    [Fact]
    public async Task RemovingItemInObservableCollectionUpdatesItemsSource()
    {
        var viewModel = new SingleVM<ObservableCollection<Team>> { Item = new ObservableCollection<Team>(_teams) };
        var datagrid = new DataGrid();
        datagrid.SetBinding(DataGrid.ItemsSourceProperty, new Binding("Item", source: viewModel));

        viewModel.Item.RemoveAt(2);
        var itemsSource = await datagrid.GetValueSafe(DataGrid.ItemsSourceProperty) as ObservableCollection<Team>;
        Assert.NotNull(itemsSource);
        Assert.Equal(_teams.Count - 1, itemsSource.Count);
        Assert.DoesNotContain(_teams[2], itemsSource);
    }

    [Fact]
    public async Task AddingItemInObservableCollectionUpdatesItemsSource()
    {
        var viewModel = new SingleVM<ObservableCollection<Team>> { Item = new ObservableCollection<Team>(_teams) };
        var datagrid = new DataGrid();
        datagrid.SetBinding(DataGrid.ItemsSourceProperty, new Binding("Item", source: viewModel));

        viewModel.Item.Add(_dummyTeam);
        var itemsSource = await datagrid.GetValueSafe(DataGrid.ItemsSourceProperty) as ObservableCollection<Team>;
        Assert.NotNull(itemsSource);
        Assert.Equal(_teams.Count + 1, itemsSource.Count);
        Assert.Contains(_dummyTeam, itemsSource);
    }

    [Fact]
    public async Task ClearingObservableCollectionUpdatesItemsSource()
    {
        var viewModel = new SingleVM<ObservableCollection<Team>> { Item = new ObservableCollection<Team>(_teams) };
        var datagrid = new DataGrid();
        datagrid.SetBinding(DataGrid.ItemsSourceProperty, new Binding("Item", source: viewModel));

        viewModel.Item.Clear();
        var itemsSource = await datagrid.GetValueSafe(DataGrid.ItemsSourceProperty) as ObservableCollection<Team>;
        Assert.NotNull(itemsSource);
        Assert.Empty(itemsSource);
    }

    [Fact]
    public void SettingItemsSourceToNull_DoesNotThrow()
    {
        var datagrid = new DataGrid { ItemsSource = _teams };
        var ex = Record.Exception(() => datagrid.ItemsSource = null!);
        Assert.Null(ex);
    }
}
