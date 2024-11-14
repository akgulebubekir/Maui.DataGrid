namespace Maui.DataGrid.Sample.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using Maui.DataGrid.Sample.Models;
using Maui.DataGrid.Sample.Tests.TestUtils;
using Xunit;

public class SelectionTest
{
    private readonly List<Team> _teams = Utils.DummyDataProvider.GetTeams();

    [Fact]
    public async Task BindingSelectedItemTriggersItemSelectedEvent()
    {
        var viewModel = new SingleVM<Team>();
        var datagrid = new DataGrid { ItemsSource = _teams };
        var eventTriggered = false;
        var teamToSelect = _teams[5];

        datagrid.ItemSelected += (s, e) =>
        {
            eventTriggered = true;
            Assert.Equal(s, datagrid);
            Assert.Empty(e.PreviousSelection);
            _ = Assert.Single(e.CurrentSelection);
            Assert.Equal([teamToSelect], e.CurrentSelection);
        };

        // set a parent to trigger  OnParentSet
        var parent = new ContentView { Content = datagrid };

        datagrid.SetBinding(DataGrid.SelectedItemProperty, new Binding("Item", source: viewModel));

        viewModel.Item = teamToSelect;
        Assert.Equal(teamToSelect, await datagrid.GetValueSafe(DataGrid.SelectedItemProperty));
        Assert.True(eventTriggered);
    }

    [Fact]
    public async Task SettingSelectedItemTriggersItemSelectedEvent()
    {
        var datagrid = new DataGrid { ItemsSource = _teams };
        var eventTriggered = false;
        var teamToSelect = _teams[5];

        datagrid.ItemSelected += (s, e) =>
        {
            eventTriggered = true;
            Assert.Equal(s, datagrid);
            Assert.Empty(e.PreviousSelection);
            _ = Assert.Single(e.CurrentSelection);
            Assert.Equal([teamToSelect], e.CurrentSelection);
        };

        // set a parent to trigger  OnParentSet
        var parent = new ContentView { Content = datagrid };

        datagrid.SelectedItem = teamToSelect;
        Assert.Equal(datagrid.SelectedItem, teamToSelect);
        Assert.Equal(teamToSelect, await datagrid.GetValueSafe(DataGrid.SelectedItemProperty));
        Assert.True(eventTriggered);
    }
}
