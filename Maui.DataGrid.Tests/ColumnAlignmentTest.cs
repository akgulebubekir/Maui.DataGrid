namespace Maui.DataGrid.Tests;

using System.Collections.ObjectModel;
using Xunit;

/// <summary>
/// Regression tests for issues #188 and #118, which are the same defect seen from two sides: the header
/// row and every data row are separate <see cref="Grid"/>s, so each resolves the shared column
/// definitions against its own available width (#188, where the rows lose a scrollbar's width that the
/// header keeps) and its own content (#118, where every row picks its own width for an Auto column).
/// <para>
/// The geometry itself cannot be asserted here: without platform handlers nothing measures or arranges,
/// so every headless measurement is zero. What is covered is the bookkeeping the fix rests on — which
/// rows are realized, how much width the header reserves for a given pair of measurements, and what
/// pinning a column does to the definition the header and the rows share.
/// </para>
/// </summary>
public class ColumnAlignmentTest
{
    [Fact]
    public void Issue188_TheHeaderReservesTheWidthTheRowsLoseToAScrollbar()
    {
        var dataGrid = CreateDataGrid();

        dataGrid.ColumnWidths.ReserveHeaderWidth(itemsHostWidth: 400, rowWidth: 383);

        Assert.Equal(17d, dataGrid.HeaderRow.Padding.Right);
    }

    [Fact]
    public void Issue188_TheHeaderReservesNothingWhenTheRowsFillTheItemsHost()
    {
        var dataGrid = CreateDataGrid();

        dataGrid.ColumnWidths.ReserveHeaderWidth(itemsHostWidth: 400, rowWidth: 400);

        Assert.Equal(0d, dataGrid.HeaderRow.Padding.Right);
    }

    [Fact]
    public void Issue188_SubPixelDifferencesAreNotTreatedAsAScrollbar()
    {
        var dataGrid = CreateDataGrid();

        // Layout rounding, not a scrollbar. Reserving it would only cause another layout pass.
        dataGrid.ColumnWidths.ReserveHeaderWidth(itemsHostWidth: 400, rowWidth: 399.7);

        Assert.Equal(0d, dataGrid.HeaderRow.Padding.Right);
    }

    [Fact]
    public void Issue188_AWidthTooLargeForAScrollbarIsIgnored()
    {
        var dataGrid = CreateDataGrid();

        // A row this much narrower than its host is not a row missing a scrollbar's width, and
        // reserving that much would push the header's own columns badly out of shape.
        dataGrid.ColumnWidths.ReserveHeaderWidth(itemsHostWidth: 400, rowWidth: 100);

        Assert.Equal(0d, dataGrid.HeaderRow.Padding.Right);
    }

    [Fact]
    public void Issue188_NothingIsReservedBeforeAnythingIsLaidOut()
    {
        var dataGrid = CreateDataGrid();

        // An unlaid-out MAUI element reports a width of -1.
        dataGrid.ColumnWidths.ReserveHeaderWidth(itemsHostWidth: -1, rowWidth: -1);

        Assert.Equal(0d, dataGrid.HeaderRow.Padding.Right);
    }

    [Fact]
    public void Issue188_TheReservationIsDroppedWhenTheLastRowGoesAway()
    {
        var dataGrid = CreateDataGrid();
        var (parent, row) = CreateRow(dataGrid, new TestItem { Name = "First" });

        dataGrid.ColumnWidths.ReserveHeaderWidth(itemsHostWidth: 400, rowWidth: 383);

        Assert.Equal(17d, dataGrid.HeaderRow.Padding.Right);

        // With no rows left there is nothing to align to, and a stale reservation would leave the
        // header of an emptied grid short.
        _ = parent.Children.Remove(row);

        Assert.Equal(0d, dataGrid.HeaderRow.Padding.Right);
    }

    [Fact]
    public void Issue118_RealizedRowsAreRegisteredWithTheGrid()
    {
        var dataGrid = CreateDataGrid();
        var (_, row) = CreateRow(dataGrid, new TestItem { Name = "First" });

        Assert.Contains(row, dataGrid.ColumnWidths.Rows);
    }

    [Fact]
    public void Issue118_RowsAreUnregisteredWhenTheCollectionViewLetsThemGo()
    {
        var dataGrid = CreateDataGrid();
        var (parent, row) = CreateRow(dataGrid, new TestItem { Name = "First" });

        _ = parent.Children.Remove(row);

        Assert.DoesNotContain(row, dataGrid.ColumnWidths.Rows);
    }

    [Fact]
    public void Issue118_TheHeaderAndTheRowsShareOneColumnDefinitionPerColumn()
    {
        var dataGrid = CreateDataGrid();
        var (_, row) = CreateRow(dataGrid, new TestItem { Name = "First" });
        var autoColumn = dataGrid.Columns[0];

        // This is what makes a single shared width possible at all: pinning the definition reaches the
        // header and every row at once.
        Assert.Same(autoColumn.ColumnDefinition, dataGrid.HeaderRow.ColumnDefinitions[0]);
        Assert.Same(autoColumn.ColumnDefinition, row.ColumnDefinitions[0]);
    }

    [Fact]
    public void Issue118_PinningAColumnGivesItAnAbsoluteWidth()
    {
        var dataGrid = CreateDataGrid();
        var autoColumn = dataGrid.Columns[0];

        ColumnWidthCoordinator.ApplySharedWidth(autoColumn, 120);

        Assert.Equal(new GridLength(120), autoColumn.ColumnDefinition!.Width);

        // The column was asked to size itself to its content, and reading it back still says so.
        Assert.True(autoColumn.Width.IsAuto);
    }

    [Fact]
    public void Issue118_AColumnWhoseCellsHaveNotBeenMeasuredKeepsItsAutoWidth()
    {
        var dataGrid = CreateDataGrid();
        var autoColumn = dataGrid.Columns[0];

        // Zero is what an unlaid-out cell measures, not a column which wants no width.
        ColumnWidthCoordinator.ApplySharedWidth(autoColumn, 0);

        Assert.True(autoColumn.ColumnDefinition!.Width.IsAuto);
    }

    [Fact]
    public void Issue118_APassLeavesStarColumnsAlone()
    {
        var dataGrid = CreateDataGrid();
        _ = CreateRow(dataGrid, new TestItem { Name = "First" });

        dataGrid.ColumnWidths.UpdateAutoWidths();

        // Star columns are already shared: they resolve from the available width, which #188's
        // reservation is what makes equal.
        Assert.True(dataGrid.Columns[1].ColumnDefinition!.Width.IsStar);
    }

    [Fact]
    public void Issue118_APassLeavesAutoColumnsAloneUntilTheirCellsMeasure()
    {
        var dataGrid = CreateDataGrid();
        _ = CreateRow(dataGrid, new TestItem { Name = "First" });

        dataGrid.ColumnWidths.UpdateAutoWidths();

        // Nothing measures without platform handlers, and a column pinned to zero would vanish.
        Assert.True(dataGrid.Columns[0].ColumnDefinition!.Width.IsAuto);
    }

    [Fact]
    public void Issue118_RecyclingARowRemeasuresTheAutoColumns()
    {
        var dataGrid = CreateDataGrid();
        var (_, row) = CreateRow(dataGrid, new TestItem { Name = "First" });

        var passes = dataGrid.ColumnWidths.AutoWidthPassCount;

        // The recycled row's cells hold different content, which the column may have to grow to fit.
        row.BindingContext = new TestItem { Name = "A much longer value than the first one" };

        Assert.True(dataGrid.ColumnWidths.AutoWidthPassCount > passes);
    }

    [Fact]
    public void Issue118_AGridWithoutAutoColumnsNeverMeasures()
    {
        var dataGrid = CreateDataGrid();
        dataGrid.Columns[0].Width = GridLength.Star;

        var passes = dataGrid.ColumnWidths.AutoWidthPassCount;

        _ = CreateRow(dataGrid, new TestItem { Name = "First" });

        // Star columns need none of this, so nobody who has not asked for Auto pays for it.
        Assert.Equal(passes, dataGrid.ColumnWidths.AutoWidthPassCount);
    }

    private static DataGrid CreateDataGrid()
    {
        var dataGrid = new DataGrid
        {
            ItemsSource = new ObservableCollection<TestItem> { new() { Name = "First" }, new() { Name = "Second" } },
            Columns =
            [
                new DataGridColumn { Title = "Name", PropertyName = nameof(TestItem.Name), Width = GridLength.Auto },
                new DataGridColumn { Title = "Also Name", PropertyName = nameof(TestItem.Name), Width = GridLength.Star },
            ],
        };

        // Stands in for the grid being loaded. The header row is what creates the column definitions,
        // and until it has run a column shared by nothing is a column no test can be about.
        dataGrid.HeaderRow.InitializeHeaderRow(force: true);

        return dataGrid;
    }

    private static (VerticalStackLayout Parent, DataGridRow Row) CreateRow(DataGrid dataGrid, object item)
    {
        var row = new DataGridRow { DataGrid = dataGrid };

        // Parenting the row is what realises it, exactly as the CollectionView does.
        var parent = new VerticalStackLayout { Children = { row } };

        row.BindingContext = item;

        return (parent, row);
    }

    private sealed class TestItem
    {
        public required string Name { get; init; }
    }
}
