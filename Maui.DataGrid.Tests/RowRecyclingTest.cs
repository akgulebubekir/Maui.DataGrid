namespace Maui.DataGrid.Tests;

using System.Collections.ObjectModel;
using System.ComponentModel;
using Xunit;

/// <summary>
/// Regression tests for issue #174: the CollectionView recycles row views by changing their binding
/// context, but <c>InitializeRow</c> used to clear and rebuild every cell on each recycle. Cells are
/// now reused, and their content bindings resolve against the row's current binding context instead
/// of a captured item.
/// </summary>
public class RowRecyclingTest
{
    [Fact]
    public void Issue174_RecyclingARowReusesItsCells()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        var row = CreateRow(dataGrid, items[0]);

        var cells = row.Children.ToList();

        row.BindingContext = items[1];

        Assert.Equal(cells, row.Children);
    }

    [Fact]
    public void Issue174_RecycledCellsShowTheNewItemsValues()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        var row = CreateRow(dataGrid, items[0]);

        Assert.Equal("First", GetCellLabel(row, 0).Text);

        row.BindingContext = items[1];

        Assert.Equal("Second", GetCellLabel(row, 0).Text);
    }

    [Fact]
    public void Issue174_RecycledCellsTrackTheNewItemAndDropTheOldOne()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        var row = CreateRow(dataGrid, items[0]);

        row.BindingContext = items[1];

        var label = GetCellLabel(row, 0);

        items[1].Name = "Second, renamed";
        Assert.Equal("Second, renamed", label.Text);

        // The cell must no longer be listening to the item it was recycled away from.
        items[0].Name = "First, renamed";
        Assert.Equal("Second, renamed", label.Text);
    }

    [Fact]
    public void Issue174_HiddenColumnsDoNotAccumulateCellsAcrossRecycles()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        dataGrid.Columns[1].IsVisible = false;

        var row = CreateRow(dataGrid, items[0]);

        row.BindingContext = items[1];
        row.BindingContext = items[0];

        Assert.Equal(2, row.Children.Count);
        Assert.Equal(0, Grid.GetColumn((BindableObject)row.Children[0]));
        Assert.Equal(2, Grid.GetColumn((BindableObject)row.Children[1]));
    }

    [Fact]
    public void Issue174_HidingAColumnRemovesItsCell()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        var row = CreateRow(dataGrid, items[0]);

        Assert.Equal(3, row.Children.Count);

        dataGrid.Columns[2].IsVisible = false;

        Assert.Equal(2, row.Children.Count);
    }

    [Fact]
    public void Issue174_RemovingAColumnRemovesItsCell()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        var row = CreateRow(dataGrid, items[0]);

        Assert.Equal(3, row.Children.Count);

        dataGrid.Columns.RemoveAt(2);

        Assert.Equal(2, row.Children.Count);
    }

    [Fact]
    public void Issue174_TemplatedCellContentFollowsTheNewItem()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        dataGrid.Columns[0].CellTemplate = new DataTemplate(() => new Editor());

        var row = CreateRow(dataGrid, items[0]);

        var cell = Assert.IsType<DataGridCell>(row.Children[0]);
        var content = Assert.IsType<Editor>(Assert.IsType<ContentView>(cell.Content).Content);

        // A templated cell binds its content's BindingContext to the column's property value.
        Assert.Equal("First", content.BindingContext);

        row.BindingContext = items[1];

        Assert.Equal("Second", content.BindingContext);
    }

    [Fact]
    public void Issue174_SwitchingToEditModeReplacesTheCellContent()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        dataGrid.Columns[0].EditCellTemplate = new DataTemplate(() => new Entry());

        var row = CreateRow(dataGrid, items[0]);

        Assert.IsType<Label>(GetCellContent(row, 0));

        row.RowToEdit = items[0];

        Assert.IsType<Entry>(GetCellContent(row, 0));
    }

    [Fact]
    public void Issue174_LeavingEditModeRestoresTheViewCell()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        dataGrid.Columns[0].EditCellTemplate = new DataTemplate(() => new Entry());

        var row = CreateRow(dataGrid, items[0]);
        row.RowToEdit = items[0];

        Assert.IsType<Entry>(GetCellContent(row, 0));

        row.RowToEdit = null!;

        Assert.IsType<Label>(GetCellContent(row, 0));
    }

    [Fact]
    public void Issue174_ASelfPathColumnBindsToTheItemItself()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        dataGrid.Columns[0].PropertyName = ".";
        dataGrid.Columns[0].CellTemplate = new DataTemplate(() => new Editor());

        var row = CreateRow(dataGrid, items[0]);

        var content = Assert.IsType<Editor>(GetCellContent(row, 0));

        Assert.Same(items[0], content.BindingContext);

        row.BindingContext = items[1];

        Assert.Same(items[1], content.BindingContext);
    }

    [Fact]
    public void Issue174_ASelfPathCellCannotReplaceTheRowsItem()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        dataGrid.Columns[0].PropertyName = ".";
        dataGrid.Columns[0].CellTemplate = new DataTemplate(() => new Editor());

        var row = CreateRow(dataGrid, items[0]);

        var content = Assert.IsType<Editor>(GetCellContent(row, 0));

        // A cell's binding context is an output of the row, never an input to it.
        content.BindingContext = new TestItem { Name = "Impostor" };

        Assert.Same(items[0], row.BindingContext);
    }

    [Fact]
    public void Issue174_AnIndexerPathColumnBindsToTheIndexedValue()
    {
        var items = new ObservableCollection<Dictionary<string, string>>
        {
            new() { ["Name"] = "First" },
            new() { ["Name"] = "Second" },
        };

        var dataGrid = new DataGrid
        {
            ItemsSource = items,
            Columns = [new DataGridColumn { Title = "Name", PropertyName = "[Name]", Width = new GridLength(100) }],
        };

        var row = CreateRow(dataGrid, items[0]);

        Assert.Equal("First", GetCellLabel(row, 0).Text);

        row.BindingContext = items[1];

        Assert.Equal("Second", GetCellLabel(row, 0).Text);
    }

    [Fact]
    public void Issue174_EditingACellWritesBackToTheRowsCurrentItem()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        InitializeColumnDataTypes(dataGrid);

        var row = CreateRow(dataGrid, items[0]);
        row.RowToEdit = items[0];

        Assert.IsType<Entry>(GetCellContent(row, 0)).Text = "First edited";

        Assert.Equal("First edited", items[0].Name);

        // Recycle the row onto the other item and edit that one.
        row.BindingContext = items[1];
        row.RowToEdit = items[1];

        Assert.IsType<Entry>(GetCellContent(row, 0)).Text = "Second edited";

        Assert.Equal("Second edited", items[1].Name);
        Assert.Equal("First edited", items[0].Name);
    }

    [Fact]
    public void Issue174_ARowWithoutAnItemIsNotTreatedAsBeingEdited()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        dataGrid.Columns[0].EditCellTemplate = new DataTemplate(() => new Entry());

        var row = CreateRow(dataGrid, items[0]);

        // The CollectionView may clear a row's item while recycling it out of view. Nothing is being
        // edited, so the empty row must not turn into a row of edit controls bound to nothing.
        row.BindingContext = null!;

        Assert.IsType<Label>(GetCellContent(row, 0));
    }

    private static ObservableCollection<TestItem> CreateItems() =>
        [new() { Name = "First" }, new() { Name = "Second" }];

    private static DataGrid CreateDataGrid(ObservableCollection<TestItem> items) =>
        new()
        {
            ItemsSource = items,
            Columns =
            [
                new DataGridColumn { Title = "Name", PropertyName = nameof(TestItem.Name), Width = new GridLength(100) },
                new DataGridColumn { Title = "Also Name", PropertyName = nameof(TestItem.Name), Width = new GridLength(100) },
                new DataGridColumn { Title = "Name Again", PropertyName = nameof(TestItem.Name), Width = new GridLength(100) },
            ],
        };

    /// <summary>
    /// Stands in for the header row, which is what resolves column data types in a realized grid.
    /// Without a data type, a column falls back to a contentless default edit cell.
    /// </summary>
    private static void InitializeColumnDataTypes(DataGrid dataGrid)
    {
        foreach (var column in dataGrid.Columns)
        {
            column.DataGrid = dataGrid;
            column.InitializeDataType();
        }
    }

    private static DataGridRow CreateRow(DataGrid dataGrid, object item)
    {
        var row = new DataGridRow { DataGrid = dataGrid };

        // Parenting the row is what realises it, exactly as the CollectionView does.
        _ = new VerticalStackLayout { Children = { row } };

        row.BindingContext = item;

        return row;
    }

    private static View GetCellContent(DataGridRow row, int childIndex)
    {
        var cell = Assert.IsType<DataGridCell>(row.Children[childIndex]);
        var cellContainer = Assert.IsType<ContentView>(cell.Content);

        Assert.NotNull(cellContainer.Content);

        return cellContainer.Content;
    }

    private static Label GetCellLabel(DataGridRow row, int childIndex) =>
        Assert.IsType<Label>(GetCellContent(row, childIndex));

    private sealed class TestItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public required string Name
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }
}
