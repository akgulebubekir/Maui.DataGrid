namespace Maui.DataGrid.Tests;

using System.Collections.ObjectModel;
using Xunit;

/// <summary>
/// Regression tests for issue #172: alternating row colors are derived from a row's index, but a
/// recycled row whose binding context did not change never recomputed them, so rows kept a stale
/// color after items were added to or removed from the source.
/// </summary>
public class RowColorsTest
{
    [Fact]
    public void Issue172_RowColorsAreRecomputed_WhenAnEarlierItemIsRemoved()
    {
        var items = CreateItems(3);
        var dataGrid = CreateDataGrid(items);
        var row = CreateRow(dataGrid, items[1]);

        Assert.Equal(Colors.Blue, row.CellBackgroundColor);
        Assert.Equal(Colors.Black, row.CellTextColor);

        items.RemoveAt(0);

        // The row's item moved from index 1 to index 0, so it takes the first palette entry.
        Assert.Equal(Colors.Red, row.CellBackgroundColor);
        Assert.Equal(Colors.White, row.CellTextColor);
    }

    [Fact]
    public void Issue172_RowColorsAreRecomputed_WhenAnEarlierItemIsInserted()
    {
        var items = CreateItems(3);
        var dataGrid = CreateDataGrid(items);
        var row = CreateRow(dataGrid, items[1]);

        Assert.Equal(Colors.Blue, row.CellBackgroundColor);

        items.Insert(0, new TestItem { Name = "Inserted" });

        // The row's item moved from index 1 to index 2, so it takes the first palette entry.
        Assert.Equal(Colors.Red, row.CellBackgroundColor);
    }

    [Fact]
    public void Issue172_RowColorsAreUnchanged_WhenALaterItemIsRemoved()
    {
        var items = CreateItems(3);
        var dataGrid = CreateDataGrid(items);
        var row = CreateRow(dataGrid, items[1]);

        items.RemoveAt(2);

        Assert.Equal(Colors.Blue, row.CellBackgroundColor);
    }

    [Fact]
    public void Issue172_RowColorsSurviveTheRowItemBeingRemoved()
    {
        var items = CreateItems(3);
        var dataGrid = CreateDataGrid(items);
        var row = CreateRow(dataGrid, items[1]);

        // The row is no longer in the grid; it must keep its last color rather than throw.
        items.RemoveAt(1);

        Assert.Equal(Colors.Blue, row.CellBackgroundColor);
    }

    private static ObservableCollection<TestItem> CreateItems(int count) =>
        [.. Enumerable.Range(0, count).Select(i => new TestItem { Name = $"Item {i}" })];

    private static DataGrid CreateDataGrid(ObservableCollection<TestItem> items) =>
        new()
        {
            ItemsSource = items,
            Columns = [new DataGridColumn { Title = "Name", PropertyName = nameof(TestItem.Name), Width = new GridLength(100) }],
            RowsBackgroundColorPalette = new PaletteCollection { Colors.Red, Colors.Blue },
            RowsTextColorPalette = new PaletteCollection { Colors.White, Colors.Black },
        };

    private static DataGridRow CreateRow(DataGrid dataGrid, object item)
    {
        var row = new DataGridRow { DataGrid = dataGrid };

        // Parenting the row is what realises it, exactly as the CollectionView does.
        _ = new VerticalStackLayout { Children = { row } };

        row.BindingContext = item;

        return row;
    }

    private sealed class TestItem
    {
        public required string Name { get; set; }
    }
}
