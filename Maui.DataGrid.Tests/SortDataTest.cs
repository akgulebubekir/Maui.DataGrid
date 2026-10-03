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

    [Theory]
    [InlineData(null)]
    public void EqualsReturnsFalseForNull(object? other)
    {
        var sortData = new SortData(1, SortingOrder.Ascendant);

        Assert.False(sortData.Equals(other));
    }

    [Fact]
    public void EqualsReturnsFalseForDifferentType()
    {
        var sortData = new SortData(1, SortingOrder.Ascendant);

        Assert.False(sortData.Equals("not a SortData"));
    }

    [Fact]
    public void EqualsReturnsTrueForSameInstance()
    {
        var sortData = new SortData(1, SortingOrder.Ascendant);

        Assert.True(sortData.Equals(sortData));
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

    [Fact]
    public void IndexAndOrderPropertiesAreMutable()
    {
        var sortData = new SortData(1, SortingOrder.Ascendant)
        {
            Index = 5,
            Order = SortingOrder.Descendant,
        };

        Assert.Equal(5, sortData.Index);
        Assert.Equal(SortingOrder.Descendant, sortData.Order);
    }

    [Theory]
    [InlineData(0, SortingOrder.Ascendant)]
    [InlineData(1, SortingOrder.Ascendant)]
    [InlineData(-1, SortingOrder.Descendant)]
    [InlineData(100, SortingOrder.Ascendant)]
    [InlineData(-100, SortingOrder.Descendant)]
    public void FromInt32_Theory(int input, SortingOrder expectedOrder)
    {
        var sortData = SortData.FromInt32(input);

        Assert.Equal(Math.Abs(input), sortData.Index);
        Assert.Equal(expectedOrder, sortData.Order);
    }
}
