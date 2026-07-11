namespace Maui.DataGrid.Sample.Tests;

using Maui.DataGrid.Extensions;
using Xunit;

public class ListExtensionsTest
{
    [Fact]
    public void TryGetItem_ValidIndexReturnsTrue()
    {
        var label = new Label();
        IList<IView> list = [label, new BoxView()];

        var result = list.TryGetItem(0, out var item);

        Assert.True(result);
        Assert.Same(label, item);
    }

    [Fact]
    public void TryGetItem_LastIndexReturnsTrue()
    {
        var boxView = new BoxView();
        IList<IView> list = [new Label(), boxView];

        var result = list.TryGetItem(1, out var item);

        Assert.True(result);
        Assert.Same(boxView, item);
    }

    [Fact]
    public void TryGetItem_NegativeIndexReturnsFalse()
    {
        IList<IView> list = [new Label()];

        var result = list.TryGetItem(-1, out var item);

        Assert.False(result);
        Assert.Null(item);
    }

    [Fact]
    public void TryGetItem_IndexOutOfRangeReturnsFalse()
    {
        IList<IView> list = [new Label()];

        var result = list.TryGetItem(5, out var item);

        Assert.False(result);
        Assert.Null(item);
    }

    [Fact]
    public void TryGetItem_EmptyListReturnsFalse()
    {
        IList<IView> list = [];

        var result = list.TryGetItem(0, out var item);

        Assert.False(result);
        Assert.Null(item);
    }

    [Fact]
    public void AddOrUpdate_AddsWhenIndexBeyondCount()
    {
        var columnDefs = new ColumnDefinitionCollection();
        var newDef = new ColumnDefinition(GridLength.Star);

        columnDefs.AddOrUpdate(newDef, 0);

        Assert.Single(columnDefs);
        Assert.Equal(newDef, columnDefs[0]);
    }

    [Fact]
    public void AddOrUpdate_UpdatesWhenDifferent()
    {
        var oldDef = new ColumnDefinition(GridLength.Auto);
        var newDef = new ColumnDefinition(GridLength.Star);
        var columnDefs = new ColumnDefinitionCollection { oldDef };

        columnDefs.AddOrUpdate(newDef, 0);

        Assert.Single(columnDefs);
        Assert.Equal(newDef, columnDefs[0]);
    }

    [Fact]
    public void AddOrUpdate_DoesNothingWhenSame()
    {
        var def = new ColumnDefinition(GridLength.Star);
        var columnDefs = new ColumnDefinitionCollection { def };

        columnDefs.AddOrUpdate(def, 0);

        Assert.Single(columnDefs);
        Assert.Same(def, columnDefs[0]);
    }

    [Fact]
    public void RemoveAfter_RemovesTrailingDefinitions()
    {
        var columnDefs = new ColumnDefinitionCollection
        {
            new(GridLength.Star),
            new(GridLength.Auto),
            new(GridLength.Star),
            new(GridLength.Auto),
        };

        columnDefs.RemoveAfter(2);

        Assert.Equal(2, columnDefs.Count);
    }

    [Fact]
    public void RemoveAfter_RemovesAllWhenIndexIsOne()
    {
        var columnDefs = new ColumnDefinitionCollection
        {
            new(GridLength.Star),
            new(GridLength.Auto),
            new(GridLength.Star),
        };

        columnDefs.RemoveAfter(1);

        Assert.Single(columnDefs);
    }

    [Fact]
    public void RemoveAfter_DoesNothingWhenIndexEqualsCount()
    {
        var columnDefs = new ColumnDefinitionCollection
        {
            new(GridLength.Star),
            new(GridLength.Auto),
        };

        columnDefs.RemoveAfter(2);

        Assert.Equal(2, columnDefs.Count);
    }

    [Fact]
    public void RemoveAfter_DoesNothingWhenIndexBeyondCount()
    {
        var columnDefs = new ColumnDefinitionCollection
        {
            new(GridLength.Star),
        };

        columnDefs.RemoveAfter(5);

        Assert.Single(columnDefs);
    }

    [Fact]
    public void TryGetItem_ExactCountIndexReturnsFalse()
    {
        IList<IView> list = [new Label(), new BoxView()];

        var result = list.TryGetItem(2, out var item);

        Assert.False(result);
        Assert.Null(item);
    }

    [Fact]
    public void AddOrUpdate_AddsMultipleSequentially()
    {
        var columnDefs = new ColumnDefinitionCollection();
        var def1 = new ColumnDefinition(GridLength.Star);
        var def2 = new ColumnDefinition(GridLength.Auto);

        columnDefs.AddOrUpdate(def1, 0);
        columnDefs.AddOrUpdate(def2, 1);

        Assert.Equal(2, columnDefs.Count);
        Assert.Equal(def1, columnDefs[0]);
        Assert.Equal(def2, columnDefs[1]);
    }
}
