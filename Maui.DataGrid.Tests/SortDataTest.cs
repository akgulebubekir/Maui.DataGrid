namespace Maui.DataGrid.Tests;

using Xunit;

public class SortDataTest
{
    [Fact]
    public void ConstructorSetsProperties()
    {
        var sortData = new SortData(2, SortingOrder.Ascendant);

        Assert.Equal(2, sortData.Index);
        Assert.Equal(SortingOrder.Ascendant, sortData.Order);
    }

    [Fact]
    public void FromInt32PositiveIndexReturnsAscendant()
    {
        var sortData = SortData.FromInt32(3);

        Assert.Equal(3, sortData.Index);
        Assert.Equal(SortingOrder.Ascendant, sortData.Order);
    }

    [Fact]
    public void FromInt32NegativeIndexReturnsDescendant()
    {
        var sortData = SortData.FromInt32(-2);

        Assert.Equal(2, sortData.Index);
        Assert.Equal(SortingOrder.Descendant, sortData.Order);
    }

    [Fact]
    public void FromInt32ZeroReturnsAscendant()
    {
        var sortData = SortData.FromInt32(0);

        Assert.Equal(0, sortData.Index);
        Assert.Equal(SortingOrder.Ascendant, sortData.Order);
    }

    [Fact]
    public void ImplicitOperatorConvertsPositiveInt()
    {
        SortData sortData = 5;

        Assert.Equal(5, sortData.Index);
        Assert.Equal(SortingOrder.Ascendant, sortData.Order);
    }

    [Fact]
    public void ImplicitOperatorConvertsNegativeInt()
    {
        SortData sortData = -1;

        Assert.Equal(1, sortData.Index);
        Assert.Equal(SortingOrder.Descendant, sortData.Order);
    }

    [Fact]
    public void EqualsReturnsTrueForSameValues()
    {
        var a = new SortData(1, SortingOrder.Ascendant);
        var b = new SortData(1, SortingOrder.Ascendant);

        Assert.True(a.Equals(b));
    }

    [Fact]
    public void EqualsReturnsFalseForDifferentIndex()
    {
        var a = new SortData(1, SortingOrder.Ascendant);
        var b = new SortData(2, SortingOrder.Ascendant);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void EqualsReturnsFalseForDifferentOrder()
    {
        var a = new SortData(1, SortingOrder.Ascendant);
        var b = new SortData(1, SortingOrder.Descendant);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void EqualsReturnsFalseForNull()
    {
        var sortData = new SortData(1, SortingOrder.Ascendant);

        Assert.False(sortData.Equals(null));
    }

    [Fact]
    public void EqualsReturnsFalseForDifferentType()
    {
        var sortData = new SortData(1, SortingOrder.Ascendant);

        Assert.False(sortData.Equals("not a SortData"));
    }

    [Fact]
    public void GetHashCodeSameForEqualObjects()
    {
        var a = new SortData(1, SortingOrder.Ascendant);
        var b = new SortData(1, SortingOrder.Ascendant);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCodeDiffersForDifferentObjects()
    {
        var a = new SortData(1, SortingOrder.Ascendant);
        var b = new SortData(2, SortingOrder.Descendant);

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }
}
