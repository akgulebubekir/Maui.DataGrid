namespace Maui.DataGrid.Sample.Tests;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Maui.DataGrid.Sample.Models;
using Maui.DataGrid.Sample.Tests.TestUtils;
using Xunit;

public class ItemsSourceTest
{
    private readonly List<Team> _teams = Utils.DummyDataProvider.GetTeams();
    private readonly Team _dummyTeam = new()
    {
        Name = "Not Exists",
        Conf = "",
        Div = "",
        Home = "",
        Last10 = "",
        Logo = "",
        Road = "",
        Streak = new Streak { NumStreak = 3, Result = Result.Lost }
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
    public async void SelectNonExistingItemNotPossible()
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
    public async void RemovingItemInObservableCollectionUpdatesItemsSource()
    {
        var viewModel = new SingleVM<ObservableCollection<Team>> { Item = new ObservableCollection<Team>(_teams) };
        var datagrid = new DataGrid();
        datagrid.SetBinding(DataGrid.ItemsSourceProperty, new Binding("Item", source: viewModel));

        viewModel.Item.RemoveAt(2);
        var itemsSource = await datagrid.GetValueSafe(DataGrid.ItemsSourceProperty) as ObservableCollection<Team>;
        Assert.NotNull(itemsSource);
        Assert.Equal(_teams.Count - 1, itemsSource!.Count);
        Assert.DoesNotContain(_teams[2], itemsSource);
    }

    [Fact]
    public async void AddingItemInObservableCollectionUpdatesItemsSource()
    {
        var viewModel = new SingleVM<ObservableCollection<Team>> { Item = new ObservableCollection<Team>(_teams) };
        var datagrid = new DataGrid();
        datagrid.SetBinding(DataGrid.ItemsSourceProperty, new Binding("Item", source: viewModel));

        viewModel.Item.Add(_dummyTeam);
        var itemsSource = await datagrid.GetValueSafe(DataGrid.ItemsSourceProperty) as ObservableCollection<Team>;
        Assert.NotNull(itemsSource);
        Assert.Equal(_teams.Count + 1, itemsSource!.Count);
        Assert.Contains(_dummyTeam, itemsSource);
    }
}
