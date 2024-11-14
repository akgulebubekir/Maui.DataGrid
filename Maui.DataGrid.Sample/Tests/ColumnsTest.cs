namespace Maui.DataGrid.Sample.Tests;

using System.Collections.ObjectModel;
using Maui.DataGrid.Sample.Tests.TestUtils;
using Xunit;

public class ColumnsTest
{
    private readonly List<DataGridColumn> _columns =
    [
        new() { Title = "Name", PropertyName = "Name" },
        new() { Title = "Won", PropertyName = "Won" },
        new() { Title = "Lost", PropertyName = "Lost" }
    ];

    [Fact]
    public async Task TestColumnsBindingFromViewModel()
    {
        var columns = new ObservableCollection<DataGridColumn>(_columns);

        var viewModel = new SingleVM<ObservableCollection<DataGridColumn>>();
        var dataGrid = new DataGrid();

        dataGrid.SetBinding(DataGrid.ColumnsProperty, new Binding("Item", source: viewModel));
        Assert.Null(dataGrid.Columns);

        var propertyChangedEventTriggered = false;
        dataGrid.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == DataGrid.ColumnsProperty.PropertyName)
            {
                propertyChangedEventTriggered = true;
            }
        };

        viewModel.Item = columns;
        Assert.Equal(columns, await dataGrid.GetValueSafe(DataGrid.ColumnsProperty));
        Assert.True(propertyChangedEventTriggered);

        columns.RemoveAt(2);

        var newColumns = await dataGrid.GetValueSafe(DataGrid.ColumnsProperty) as ObservableCollection<DataGridColumn>;
        Assert.NotNull(newColumns);
        Assert.NotNull(dataGrid.Columns);
        Assert.Equal(2, dataGrid.Columns.Count);
        Assert.Equal("Name", dataGrid.Columns[0].Title);
        Assert.Equal("Won", dataGrid.Columns[1].Title);
    }

    [Fact]
    public async Task SortOrderBindingOnlyWorksWhenLoaded()
    {
        var dataGrid = new DataGrid
        {
            Columns = new ObservableCollection<DataGridColumn>(_columns),
        };

        var viewModel = new SingleVM<SortData?>();

        dataGrid.SetBinding(DataGrid.SortedColumnIndexProperty, new Binding("Item", source: viewModel));
        Assert.Null(dataGrid.SortedColumnIndex);

        var propertyChangedEventTriggered = false;
        dataGrid.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == DataGrid.SortedColumnIndexProperty.PropertyName)
            {
                propertyChangedEventTriggered = true;
            }
        };

        viewModel.Item = -1;

        var sortIndex = (SortData)await dataGrid.GetValueSafe(DataGrid.SortedColumnIndexProperty);

        Assert.Equal(1, sortIndex.Index);
        Assert.Equal(SortingOrder.Descendant, sortIndex.Order);
        Assert.True(propertyChangedEventTriggered);
    }
}
