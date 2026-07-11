namespace Maui.DataGrid.Sample.Tests;

using Maui.DataGrid.Sample.Tests.TestUtils;
using Xunit;

public class DataGridColumnTest
{
    [Fact]
    public void DefaultPropertyValues()
    {
        var column = new DataGridColumn();

        Assert.Equal(string.Empty, column.Title);
        Assert.Equal(GridLength.Star, column.Width);
        Assert.True(column.IsVisible);
        Assert.True(column.SortingEnabled);
        Assert.True(column.FilteringEnabled);
        Assert.Equal(LineBreakMode.WordWrap, column.LineBreakMode);
        Assert.Equal(LayoutOptions.Center, column.HorizontalContentAlignment);
        Assert.Equal(LayoutOptions.Center, column.VerticalContentAlignment);
        Assert.Null(column.PropertyName);
        Assert.Null(column.StringFormat);
        Assert.Null(column.CellTemplate);
    }

    [Fact]
    public void IsSortableReturnsTrueForComparableType()
    {
        var column = new DataGridColumn { PropertyName = "Value" };

        var teams = new List<TestItem> { new() { Value = 1 } };
        var dataGrid = new DataGrid
        {
            ItemsSource = teams,
            Columns = [column]
        };

        column.DataGrid = dataGrid;
        column.InitializeDataType();

        Assert.True(column.IsSortable());
    }

    [Fact]
    public void IsSortableReturnsFalseForNonComparableType()
    {
        var column = new DataGridColumn { PropertyName = "NonComparable" };

        var items = new List<TestItemWithNonComparable> { new() { NonComparable = new object() } };
        var dataGrid = new DataGrid
        {
            ItemsSource = items,
            Columns = [column]
        };

        column.DataGrid = dataGrid;
        column.InitializeDataType();

        Assert.False(column.IsSortable());
    }

    [Fact]
    public void TitlePropertyBinding()
    {
        var column = new DataGridColumn();
        column.CheckPropertyBindingWorks(DataGridColumn.TitleProperty, "Column A", "Column B");
    }

    [Fact]
    public void WidthPropertyBinding()
    {
        var column = new DataGridColumn();
        column.CheckPropertyBindingWorks(DataGridColumn.WidthProperty, new GridLength(100), new GridLength(200));
    }

    [Fact]
    public void IsVisiblePropertyBinding()
    {
        var column = new DataGridColumn();
        column.CheckPropertyBindingWorks(DataGridColumn.IsVisibleProperty, false, true);
    }

    [Fact]
    public void SortingEnabledPropertyBinding()
    {
        var column = new DataGridColumn();
        column.CheckPropertyBindingWorks(DataGridColumn.SortingEnabledProperty, false, true);
    }

    public class TestItem
    {
        public int Value { get; set; }
    }

    public class TestItemWithNonComparable
    {
        public object? NonComparable { get; set; }
    }
}
