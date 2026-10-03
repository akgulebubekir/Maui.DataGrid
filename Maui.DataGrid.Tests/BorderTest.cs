namespace Maui.DataGrid.Tests;

using System.Collections.ObjectModel;
using Xunit;

/// <summary>
/// Regression tests for issue #179: cell borders are faked rather than drawn — a cell is padded by half
/// the border thickness over a surface painted in <see cref="DataGrid.BorderColor"/>, and that surface is
/// the row itself. At zero thickness there is no border to draw, but the surface stayed opaque, so it
/// showed through wherever <c>Star</c> column rounding left a sub-pixel gap: borders which appeared,
/// vanished, and moved as the window resized. The same opaque surface is what #225 cannot see through.
/// </summary>
public class BorderTest
{
    [Fact]
    public void Issue179_TheRowBackingIsTransparentWhenThereIsNoBorder()
    {
        var dataGrid = CreateDataGrid();
        dataGrid.BorderThickness = new Thickness(0);

        var row = CreateRow(dataGrid);

        Assert.Equal(Colors.Transparent, row.BackgroundColor);
    }

    [Fact]
    public void Issue179_TheRowBackingIsTheBorderColorWhenThereIsABorder()
    {
        var dataGrid = CreateDataGrid();

        var row = CreateRow(dataGrid);

        // The default thickness draws borders, and this surface is what they are painted in.
        Assert.Equal(dataGrid.BorderColor, row.BackgroundColor);
    }

    [Fact]
    public void Issue179_TheBackingFollowsTheThicknessBeingSetToZero()
    {
        var dataGrid = CreateDataGrid();
        var row = CreateRow(dataGrid);

        dataGrid.BorderThickness = new Thickness(0);

        Assert.Equal(Colors.Transparent, row.BackgroundColor);
    }

    [Fact]
    public void Issue179_TheBackingFollowsTheThicknessBeingRestored()
    {
        var dataGrid = CreateDataGrid();
        dataGrid.BorderThickness = new Thickness(0);
        var row = CreateRow(dataGrid);

        dataGrid.BorderThickness = new Thickness(2);

        Assert.Equal(dataGrid.BorderColor, row.BackgroundColor);
    }

    [Fact]
    public void Issue179_TheBackingStillFollowsTheBorderColor()
    {
        var dataGrid = CreateDataGrid();
        var row = CreateRow(dataGrid);

        dataGrid.BorderColor = Colors.Red;

        Assert.Equal(Colors.Red, row.BackgroundColor);
    }

    [Fact]
    public void Issue179_APartlyZeroThicknessStillPaintsItsRemainingBorders()
    {
        var dataGrid = CreateDataGrid();

        // Vertical borders off, horizontal borders on. The surface is shared by both, so it has to stay
        // opaque; only a thickness of zero on every edge means nothing is being asked for.
        dataGrid.BorderThickness = new Thickness(0, 1);

        var row = CreateRow(dataGrid);

        Assert.Equal(dataGrid.BorderColor, row.BackgroundColor);
    }

    [Fact]
    public void Issue179_TheHeaderBackingIsTransparentWhenThereIsNoBorder()
    {
        var dataGrid = CreateDataGrid();

        dataGrid.BorderThickness = new Thickness(0);

        Assert.Equal(Colors.Transparent, dataGrid.HeaderRow.BackgroundColor);
    }

    [Fact]
    public void Issue179_TheHeaderCellBackingIsTransparentWhenThereIsNoBorder()
    {
        var dataGrid = CreateDataGrid();
        dataGrid.BorderThickness = new Thickness(0);

        dataGrid.HeaderRow.InitializeHeaderRow(force: true);

        Assert.Equal(Colors.Transparent, dataGrid.Columns[0].HeaderCell!.BackgroundColor);
    }

    [Fact]
    public void Issue179_TheRowCellBackingIsTransparentWhenThereIsNoBorder()
    {
        var dataGrid = CreateDataGrid();
        dataGrid.BorderThickness = new Thickness(0);

        var row = CreateRow(dataGrid);

        Assert.Equal(Colors.Transparent, row.GetCellForColumn(dataGrid.Columns[0])!.BackgroundColor);
    }

    [Fact]
    public void Issue179_HidingTheHeaderBordersLeavesNoBackingBehind()
    {
        var dataGrid = CreateDataGrid();

        // Dropping the binding is not enough on its own: a bindable property keeps its last value, so
        // the cell held onto the border colour it had been given and bled it through the same gaps.
        dataGrid.HeaderBordersVisible = false;

        dataGrid.HeaderRow.InitializeHeaderRow(force: true);

        Assert.Equal(Colors.Transparent, dataGrid.Columns[0].HeaderCell!.BackgroundColor);
    }

    private static DataGrid CreateDataGrid() =>
        new()
        {
            ItemsSource = new ObservableCollection<TestItem> { new() { Name = "First" } },
            Columns = [new DataGridColumn { Title = "Name", PropertyName = nameof(TestItem.Name), Width = new GridLength(100) }],
            BorderColor = Colors.Blue,
        };

    private static DataGridRow CreateRow(DataGrid dataGrid)
    {
        var row = new DataGridRow { DataGrid = dataGrid };

        // Parenting the row is what realises it, exactly as the CollectionView does.
        _ = new VerticalStackLayout { Children = { row } };

        row.BindingContext = new TestItem { Name = "First" };

        return row;
    }

    private sealed class TestItem
    {
        public required string Name { get; init; }
    }
}
