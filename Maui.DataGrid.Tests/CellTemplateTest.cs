namespace Maui.DataGrid.Tests;

using Xunit;

/// <summary>
/// Regression tests for issue #230: a <see cref="DataTemplateSelector"/> assigned to
/// <see cref="DataGridColumn.CellTemplate"/> or <see cref="DataGridColumn.EditCellTemplate"/>
/// must be resolved per row via <see cref="DataTemplateSelector.SelectTemplate"/>.
/// </summary>
public class CellTemplateTest
{
    [Fact]
    public void Issue230_CellTemplateSelector_SelectsTemplatePerRow()
    {
        var positive = new TestItem { Value = 1 };
        var negative = new TestItem { Value = -1 };
        var dataGrid = CreateDataGrid([positive, negative], cellTemplate: new TestTemplateSelector());

        Assert.IsType<Label>(GetCellContent(dataGrid, positive));
        Assert.IsType<Button>(GetCellContent(dataGrid, negative));
    }

    [Fact]
    public void Issue230_EditCellTemplateSelector_SelectsTemplatePerRow()
    {
        var positive = new TestItem { Value = 1 };
        var negative = new TestItem { Value = -1 };
        var dataGrid = CreateDataGrid([positive, negative], editCellTemplate: new TestTemplateSelector());

        Assert.IsType<Label>(GetCellContent(dataGrid, positive, rowToEdit: positive));
        Assert.IsType<Button>(GetCellContent(dataGrid, negative, rowToEdit: negative));
    }

    [Fact]
    public void Issue230_CellTemplateSelectorIsReSelectedWhenTheRowIsRecycled()
    {
        var positive = new TestItem { Value = 1 };
        var negative = new TestItem { Value = -1 };
        var dataGrid = CreateDataGrid([positive, negative], cellTemplate: new TestTemplateSelector());

        var row = new DataGridRow { DataGrid = dataGrid, BindingContext = positive };

        Assert.IsType<Label>(GetCellContent(row));

        // Rows are recycled onto other items rather than rebuilt (issue #174),
        // so the selector has to be consulted again for the new item.
        row.BindingContext = negative;

        Assert.IsType<Button>(GetCellContent(row));
    }

    [Fact]
    public void Issue230_PlainCellTemplateIsStillUsedAsIs()
    {
        var item = new TestItem { Value = 1 };
        var dataGrid = CreateDataGrid([item], cellTemplate: new DataTemplate(() => new Editor()));

        Assert.IsType<Editor>(GetCellContent(dataGrid, item));
    }

    private static DataGrid CreateDataGrid(
        IList<TestItem> items,
        DataTemplate? cellTemplate = null,
        DataTemplate? editCellTemplate = null)
    {
        var column = new DataGridColumn
        {
            Title = "Value",
            PropertyName = nameof(TestItem.Value),

            // An explicit width creates the column's ColumnDefinition, which rows require.
            Width = new GridLength(100),
            CellTemplate = cellTemplate,
            EditCellTemplate = editCellTemplate,
        };

        return new DataGrid
        {
            ItemsSource = items,
            Columns = [column],
        };
    }

    private static View GetCellContent(DataGrid dataGrid, object item, object? rowToEdit = null)
    {
        var row = new DataGridRow { DataGrid = dataGrid };

        if (rowToEdit != null)
        {
            row.RowToEdit = rowToEdit;
        }

        // Setting the binding context is what builds the row's cells.
        row.BindingContext = item;

        return GetCellContent(row);
    }

    private static View GetCellContent(DataGridRow row)
    {
        var cell = Assert.IsType<DataGridCell>(Assert.Single(row.Children));
        var cellContainer = Assert.IsType<ContentView>(cell.Content);

        Assert.NotNull(cellContainer.Content);

        return cellContainer.Content;
    }

    private sealed class TestItem
    {
        public int Value { get; set; }
    }

    private sealed class TestTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _positiveTemplate = new(() => new Label());
        private readonly DataTemplate _negativeTemplate = new(() => new Button());

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
            item is TestItem { Value: > 0 } ? _positiveTemplate : _negativeTemplate;
    }
}
