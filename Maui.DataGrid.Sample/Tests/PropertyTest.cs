namespace Maui.DataGrid.Sample.Tests;

using Maui.DataGrid.Sample.Tests.TestUtils;
using Microsoft.Maui.Controls;
using Xunit;

public class PropertyTest
{
    private static readonly PaletteCollection Palette1 = [Colors.Orange, Colors.Red];
    private static readonly PaletteCollection Palette2 = [Colors.Lime, Colors.Green];
    private static readonly string[] TestStringArray = ["a", "b", "c"];
    private static readonly string[] UpdatedStringArray = ["d", "e"];

    [Fact]
    public void TestPropertiesSetsProperly()
    {
        TestProperty(DataGrid.ActiveRowColorProperty, Colors.Orange, Colors.Red);
        TestProperty(DataGrid.BorderColorProperty, Colors.Orange, Colors.Red);
        TestProperty(DataGrid.BorderThicknessProperty, new Thickness(1, 2, 3, 4), new Thickness(4, 5, 6, 7));
        TestProperty(DataGrid.FontFamilyProperty, "OpenSansSemibold", "OpenSansRegular");
        TestProperty(DataGrid.FontSizeProperty, 10.0, 11.0);
        TestProperty(DataGrid.FooterBackgroundProperty, Colors.Orange, Colors.Red);
        TestProperty(DataGrid.FooterHeightProperty, 42, 44);
        TestProperty(DataGrid.HeaderBackgroundProperty, Colors.Orange, Colors.Red);
        TestProperty(DataGrid.HeaderBordersVisibleProperty, true, false);
        TestProperty(DataGrid.HeaderHeightProperty, 42, 44);
        TestProperty(DataGrid.IsRefreshingProperty, true, false);
        TestProperty(DataGrid.SortingEnabledProperty, true, false);
        TestProperty(DataGrid.ItemSizingStrategyProperty, ItemSizingStrategy.MeasureAllItems, ItemSizingStrategy.MeasureFirstItem);
        TestProperty(DataGrid.ItemsSourceProperty, TestStringArray, UpdatedStringArray);
        TestProperty(DataGrid.NoDataViewProperty, new ContentView { Background = Colors.Aqua }, new ContentView { Background = Colors.Lime });
        TestProperty(DataGrid.PageSizeVisibleProperty, true, false);
        TestProperty(DataGrid.PaginationEnabledProperty, true, false);
        TestProperty(DataGrid.PullToRefreshCommandParameterProperty, "param1", "param2");
        TestProperty(DataGrid.PullToRefreshCommandProperty, new Command(Command1), new Command(Command2));
        TestProperty(DataGrid.RefreshColorProperty, Colors.Orange, Colors.Red);
        TestProperty(DataGrid.RefreshingEnabledProperty, true, false);
        TestProperty(DataGrid.RowHeightProperty, 42, 44);
        TestProperty(DataGrid.RowsBackgroundColorPaletteProperty, Palette1, Palette2);
        TestProperty(DataGrid.RowsTextColorPaletteProperty, Palette1, Palette2);
        TestProperty(DataGrid.SelectionModeProperty, SelectionMode.Single, SelectionMode.Multiple);
    }

    internal static void TestProperty<T>(BindableProperty property, T testValue, T updatedValue)
        where T : notnull
    {
        var dataGrid = new DataGrid();
        dataGrid.CheckPropertyBindingWorks(property, testValue, updatedValue);

        var anotherDataGrid = new DataGrid();
        anotherDataGrid.CheckStyleSettingWorks(property, testValue);
    }

    private void Command1()
    {
    }

    private void Command2()
    {
    }
}
