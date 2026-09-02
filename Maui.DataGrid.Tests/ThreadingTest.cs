namespace Maui.DataGrid.Tests;

using System.Collections.ObjectModel;
using Maui.DataGrid.Tests.TestUtils;
using Xunit;

/// <summary>
/// Regression tests for issue #185: adding items to <see cref="DataGrid.ItemsSource"/> from a background
/// thread used to sort, filter and paginate on that same thread. That work touches the pagination
/// controls and the items the CollectionView is displaying, which throws on a real platform. The work is
/// now marshalled to the UI thread.
/// </summary>
public class ThreadingTest
{
    [Fact]
    public async Task Issue185_ItemsAddedOffTheUiThreadAreAppliedOnTheUiThread()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);

        using var mainThread = TestBootstrap.Dispatcher.SimulateMainThread();

        await AddOffTheUiThread(items, "Third");

        // The background thread must have left the grid alone.
        Assert.Equal(2, dataGrid.InternalItems.Count);

        TestBootstrap.Dispatcher.RunQueuedDispatches();

        Assert.Equal(3, dataGrid.InternalItems.Count);
        Assert.Equal("Third", Assert.IsType<TestItem>(dataGrid.InternalItems[2]).Name);
    }

    [Fact]
    public async Task Issue185_EveryOffThreadMutationIsApplied()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);

        using var mainThread = TestBootstrap.Dispatcher.SimulateMainThread();

        await AddOffTheUiThread(items, "Third", "Fourth", "Fifth");

        Assert.Equal(2, dataGrid.InternalItems.Count);

        TestBootstrap.Dispatcher.RunQueuedDispatches();

        Assert.Equal(5, dataGrid.InternalItems.Count);
    }

    [Fact]
    public async Task Issue185_PaginationIsRecalculatedOnTheUiThread()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);
        dataGrid.PaginationEnabled = true;
        dataGrid.PageSize = 2;

        using var mainThread = TestBootstrap.Dispatcher.SimulateMainThread();

        Assert.Equal(1, dataGrid.PageCount);

        // PageCount is the property whose setter reaches for the pagination stepper.
        await AddOffTheUiThread(items, "Third");

        Assert.Equal(1, dataGrid.PageCount);

        TestBootstrap.Dispatcher.RunQueuedDispatches();

        Assert.Equal(2, dataGrid.PageCount);
        Assert.Equal(2, dataGrid.InternalItems.Count);
    }

    [Fact]
    public void Issue185_ItemsAddedOnTheUiThreadAreAppliedImmediately()
    {
        var items = CreateItems();
        var dataGrid = CreateDataGrid(items);

        using var mainThread = TestBootstrap.Dispatcher.SimulateMainThread();

        items.Add(new TestItem { Name = "Third" });

        // Nothing needs marshalling here, so the grid is up to date as soon as the collection is.
        Assert.Equal(3, dataGrid.InternalItems.Count);
    }

    /// <summary>
    /// Adds items on a pool thread, the way a background worker filling a collection does, and rethrows
    /// anything which went wrong there.
    /// </summary>
    private static Task AddOffTheUiThread(ObservableCollection<TestItem> items, params string[] names) =>
        Task.Run(() =>
        {
            foreach (var name in names)
            {
                items.Add(new TestItem { Name = name });
            }
        });

    private static ObservableCollection<TestItem> CreateItems() =>
        [new() { Name = "First" }, new() { Name = "Second" }];

    private static DataGrid CreateDataGrid(ObservableCollection<TestItem> items) =>
        new()
        {
            ItemsSource = items,
            Columns = [new DataGridColumn { Title = "Name", PropertyName = nameof(TestItem.Name), Width = new GridLength(100) }],
        };

    private sealed class TestItem
    {
        public required string Name { get; init; }
    }
}
