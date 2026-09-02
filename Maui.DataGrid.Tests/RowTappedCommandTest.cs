namespace Maui.DataGrid.Tests;

using System.Windows.Input;
using Xunit;

/// <summary>
/// Regression tests for issue #231: <see cref="DataGrid.RowTappedCommand"/> used to be invoked
/// only from the selection-changed handler, so re-tapping the selected row — or tapping any row
/// with <see cref="SelectionMode.None"/> — never executed it. Opting into
/// <see cref="RowTappedCommandMode.Tap"/> makes every row tap execute it with the tapped item.
/// </summary>
public class RowTappedCommandTest
{
    [Fact]
    public void Issue231_RowTappedCommandMode_DefaultsToSelectionChanged()
    {
        var dataGrid = new DataGrid();

        Assert.Equal(RowTappedCommandMode.SelectionChanged, dataGrid.RowTappedCommandMode);
    }

    [Fact]
    public void Issue231_TapMode_ExecutesCommandWithTappedItemOnEveryTap()
    {
        var items = CreateItems();
        var executions = new List<object?>();
        var dataGrid = CreateDataGrid(items, RowTappedCommandMode.Tap, executions);

        var row = CreateRow(dataGrid, items[1]);

        TapRow(row);
        TapRow(row);

        Assert.Equal([items[1], items[1]], executions);
    }

    [Fact]
    public void Issue231_TapMode_ExecutesCommandWhenSelectionIsDisabled()
    {
        var items = CreateItems();
        var executions = new List<object?>();
        var dataGrid = CreateDataGrid(items, RowTappedCommandMode.Tap, executions);
        dataGrid.SelectionMode = SelectionMode.None;

        TapRow(CreateRow(dataGrid, items[0]));

        Assert.Equal([items[0]], executions);
    }

    [Fact]
    public void Issue231_TapMode_DoesNotExecuteCommandOnSelectionChange()
    {
        var items = CreateItems();
        var executions = new List<object?>();
        var dataGrid = CreateDataGrid(items, RowTappedCommandMode.Tap, executions);

        // A parent is required for the DataGrid to listen to its CollectionView's selection.
        _ = new ContentView { Content = dataGrid };

        dataGrid.SelectedItem = items[1];

        Assert.Empty(executions);
    }

    [Fact]
    public void Issue231_SelectionChangedMode_StillExecutesCommandWithSelectionChangedEventArgs()
    {
        var items = CreateItems();
        var executions = new List<object?>();
        var dataGrid = CreateDataGrid(items, RowTappedCommandMode.SelectionChanged, executions);

        _ = new ContentView { Content = dataGrid };

        dataGrid.SelectedItem = items[1];

        var args = Assert.IsType<SelectionChangedEventArgs>(Assert.Single(executions));
        Assert.Equal([items[1]], args.CurrentSelection);
    }

    [Fact]
    public void Issue231_SelectionChangedMode_AttachesNoTapGestureToRows()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items, RowTappedCommandMode.SelectionChanged, []);

        var row = CreateRow(dataGrid, items[0]);

        Assert.Empty(row.GestureRecognizers);
    }

    [Fact]
    public void Issue231_SwitchingToTapModeAfterRowsExist_EnablesRowTaps()
    {
        var items = CreateItems();
        var executions = new List<object?>();
        var dataGrid = CreateDataGrid(items, RowTappedCommandMode.SelectionChanged, executions);

        var row = CreateRow(dataGrid, items[0]);
        dataGrid.RowTappedCommandMode = RowTappedCommandMode.Tap;

        TapRow(row);

        Assert.Equal([items[0]], executions);
    }

    [Fact]
    public void Issue231_TapMode_HonoursCanExecute()
    {
        var items = CreateItems();
        var executions = new List<object?>();
        var dataGrid = CreateDataGrid(items, RowTappedCommandMode.Tap, executions, canExecute: _ => false);

        TapRow(CreateRow(dataGrid, items[0]));

        Assert.Empty(executions);
    }

    private static List<TestItem> CreateItems() =>
        [new() { Name = "First" }, new() { Name = "Second" }];

    private static DataGrid CreateDataGrid(
        IList<TestItem> items,
        RowTappedCommandMode mode,
        IList<object?> executions,
        Func<object?, bool>? canExecute = null)
    {
        ICommand command = canExecute == null
            ? new Command<object?>(executions.Add)
            : new Command<object?>(executions.Add, canExecute);

        return new DataGrid
        {
            ItemsSource = items,
            Columns = [new DataGridColumn { Title = "Name", PropertyName = nameof(TestItem.Name), Width = new GridLength(100) }],
            RowTappedCommandMode = mode,
            RowTappedCommand = command,
        };
    }

    private static DataGridRow CreateRow(DataGrid dataGrid, object item)
    {
        var row = new DataGridRow { DataGrid = dataGrid };

        // Parenting the row is what realises it, exactly as the CollectionView does.
        _ = new VerticalStackLayout { Children = { row } };

        row.BindingContext = item;

        return row;
    }

    private static void TapRow(DataGridRow row)
    {
        var tapGesture = Assert.IsType<TapGestureRecognizer>(Assert.Single(row.GestureRecognizers));

        Assert.NotNull(tapGesture.Command);

        // Stands in for the platform raising the gesture.
        tapGesture.Command.Execute(tapGesture.CommandParameter);
    }

    private sealed class TestItem
    {
        public required string Name { get; set; }
    }
}
