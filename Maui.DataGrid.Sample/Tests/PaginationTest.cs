namespace Maui.DataGrid.Sample.Tests;

using System.Collections.Generic;
using Maui.DataGrid.Sample.Models;
using Maui.DataGrid.Sample.Tests.TestUtils;
using Xunit;

public class PaginationTest
{
    private readonly List<Team> _teams = Utils.DummyDataProvider.GetTeams();

    [Fact]
    public void PageCountDoesNotChangesWithBinding()
    {
        var dataGrid = new DataGrid { ItemsSource = _teams, PageSize = 10 };

        Assert.Equal(2, dataGrid.PageCount);

        var countViewModel = new SingleVM<int>();
        dataGrid.SetBinding(DataGrid.PageCountProperty, new Binding("Item", source: countViewModel));

        countViewModel.Item = 1;
        Assert.Equal(2, dataGrid.PageCount);
    }

    [Fact]
    public void PageNumberDoesNotExceedsLimit()
    {
        var dataGrid = new DataGrid { ItemsSource = _teams, PageSize = 10 };

        Assert.Equal(1, dataGrid.PageNumber);

        dataGrid.PageNumber = 2;
        Assert.Equal(2, dataGrid.PageNumber);

        dataGrid.PageNumber = 3;
        Assert.Equal(2, dataGrid.PageNumber);
    }

    [Fact]
    public void PageSizeAllowsMorePageNumber()
    {
        var dataGrid = new DataGrid { ItemsSource = _teams, PageSize = 10 };

        Assert.Equal(1, dataGrid.PageNumber);

        dataGrid.PageSize = 5;

        dataGrid.PageNumber = 3;
        Assert.Equal(3, dataGrid.PageNumber);

        dataGrid.PageNumber = 30;
        Assert.Equal(3, dataGrid.PageNumber);
    }

    [Fact]
    public void PageNumberCannotBeNegative()
    {
        var dataGrid = new DataGrid { ItemsSource = _teams, PageSize = 10 };

        Assert.Equal(1, dataGrid.PageNumber);

        dataGrid.PageNumber = -1;
        Assert.Equal(1, dataGrid.PageNumber);
    }

    [Fact]
    public void PageSizeCannotBeNegative()
    {
        var dataGrid = new DataGrid { ItemsSource = _teams, PageSize = 10 };

        Assert.Equal(10, dataGrid.PageSize);
        dataGrid.PageSize = -1;
        Assert.Equal(10, dataGrid.PageSize);
    }

    [Fact]
    public void PageNumberResetsWhenPageSizeChanges()
    {
#pragma warning disable IDE0017 // Simplify object initialization
        var dataGrid = new DataGrid { ItemsSource = _teams, PageSize = 6 };
        dataGrid.PageNumber = 3;
#pragma warning restore IDE0017 // Simplify object initialization
        Assert.Equal(3, dataGrid.PageNumber);
        dataGrid.PageSize = 5;
        Assert.Equal(1, dataGrid.PageNumber);
    }

    [Fact]
    public void PageSizeListUpdatedWithUnknownNumber()
    {
        var dataGrid = new DataGrid { ItemsSource = _teams, PageSize = 6 };

        Assert.DoesNotContain(7, dataGrid.PageSizeList);
        dataGrid.PageSize = 7;
        Assert.Contains(7, dataGrid.PageSizeList);
    }
}
