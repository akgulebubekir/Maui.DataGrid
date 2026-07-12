namespace Maui.DataGrid.Tests;

using System.Collections.Generic;
using Maui.DataGrid.Tests.Models;
using Maui.DataGrid.Tests.TestUtils;
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

    [Fact]
    public void PageCount_WithFewerItemsThanPageSize_ReturnsOne()
    {
        var dataGrid = new DataGrid { ItemsSource = _teams, PageSize = 1000 };

        Assert.Equal(1, dataGrid.PageCount);
    }

    [Fact]
    public void PageCount_WithExactMultiple_ReturnsExactPageCount()
    {
        // 15 teams, PageSize = 5 → ceiling(15/5) = 3 exactly
        var dataGrid = new DataGrid { ItemsSource = _teams, PageSize = 5 };

        Assert.Equal(3, dataGrid.PageCount);
    }

    [Fact]
    public void PageSizeList_ContainsDefaultPageSizes()
    {
        var dataGrid = new DataGrid();

        Assert.Contains(5, dataGrid.PageSizeList);
        Assert.Contains(10, dataGrid.PageSizeList);
        Assert.Contains(50, dataGrid.PageSizeList);
        Assert.Contains(100, dataGrid.PageSizeList);
    }

    [Fact]
    public void PageNumber_IsOne_WhenPageSizeExceedsItemCount()
    {
#pragma warning disable IDE0017 // Simplify object initialization
        var dataGrid = new DataGrid { ItemsSource = _teams, PageSize = 5 };
        dataGrid.PageNumber = 3;
#pragma warning restore IDE0017 // Simplify object initialization
        Assert.Equal(3, dataGrid.PageNumber);

        dataGrid.PageSize = 1000;

        // PageCount drops to 1, so PageNumber must reset to 1
        Assert.Equal(1, dataGrid.PageNumber);
    }
}
