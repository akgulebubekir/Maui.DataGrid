namespace Maui.DataGrid.Tests;

using Maui.DataGrid.Tests.TestUtils;
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
        Assert.Null(column.HeaderToolTip);
    }

    [Fact]
    public void IsSortableReturnsTrueForComparableType()
    {
        var column = new DataGridColumn { PropertyName = "Value" };

        var teams = new List<TestItem> { new() { Value = 1 } };
        var dataGrid = new DataGrid
        {
            ItemsSource = teams,
            Columns = [column],
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
            Columns = [column],
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
    public void HeaderToolTipPropertyBinding()
    {
        var column = new DataGridColumn();
        column.CheckPropertyBindingWorks(DataGridColumn.HeaderToolTipProperty, "Games won at home", "Games won away");
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

    [Fact]
    public void PropertyNamePropertyBinding()
    {
        var column = new DataGridColumn();
        column.CheckPropertyBindingWorks(DataGridColumn.PropertyNameProperty, "Name", "Score");
    }

    [Fact]
    public void LineBreakModePropertyBinding()
    {
        var column = new DataGridColumn();
        column.CheckPropertyBindingWorks(DataGridColumn.LineBreakModeProperty, LineBreakMode.NoWrap, LineBreakMode.TailTruncation);
    }

    [Fact]
    public void HorizontalContentAlignmentPropertyBinding()
    {
        var column = new DataGridColumn();
        column.CheckPropertyBindingWorks(DataGridColumn.HorizontalContentAlignmentProperty, LayoutOptions.Start, LayoutOptions.End);
    }

    [Fact]
    public void VerticalContentAlignmentPropertyBinding()
    {
        var column = new DataGridColumn();
        column.CheckPropertyBindingWorks(DataGridColumn.VerticalContentAlignmentProperty, LayoutOptions.Start, LayoutOptions.End);
    }

    [Fact]
    public void PaddingPropertyBinding()
    {
        var column = new DataGridColumn();
        column.CheckPropertyBindingWorks(DataGridColumn.PaddingProperty, new Thickness(4), new Thickness(8));
    }

    [Fact]
    public void FilterText_DefaultIsNull()
    {
        var column = new DataGridColumn();
        Assert.Null(column.FilterText);
    }

    [Fact]
    public void FilterText_CanBeSetDirectly()
    {
        var column = new DataGridColumn { FilterText = "search" };
        Assert.Equal("search", column.FilterText);
    }

    [Fact]
    public void IsSortable_ReturnsFalse_WhenDataGridIsNull()
    {
        var column = new DataGridColumn { PropertyName = "Value" };

        // No DataGrid assigned; ItemsSource check short-circuits to false
        Assert.False(column.IsSortable());
    }

    [Fact]
    public void IsSortable_ReturnsFalse_WhenItemsSourceIsNull()
    {
        var column = new DataGridColumn { PropertyName = "Value" };
        var dataGrid = new DataGrid(); // ItemsSource is null by default
        column.DataGrid = dataGrid;

        Assert.False(column.IsSortable());
    }

    [Fact]
    public void IsSortable_CachesResult()
    {
        var column = new DataGridColumn { PropertyName = "Value" };
        var items = new List<TestItem> { new() { Value = 1 } };
        var dataGrid = new DataGrid
        {
            ItemsSource = items,
            Columns = [column],
        };

        column.DataGrid = dataGrid;
        column.InitializeDataType();

        var firstCall = column.IsSortable();
        var secondCall = column.IsSortable();

        Assert.True(firstCall);
        Assert.Equal(firstCall, secondCall);
    }

    private sealed class TestItem
    {
        public int Value { get; set; }
    }

    private sealed class TestItemWithNonComparable
    {
        public object? NonComparable { get; set; }
    }
}
