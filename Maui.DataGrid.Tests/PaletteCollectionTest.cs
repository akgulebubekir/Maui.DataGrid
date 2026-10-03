namespace Maui.DataGrid.Tests;

using Xunit;

public class PaletteCollectionTest
{
    [Fact]
    public void EmptyPaletteCollection()
    {
        var palette = new PaletteCollection();

        Assert.Empty(palette);
        Assert.Equal(palette.GetColor(0, "item"), Colors.White);
    }

    [Fact]
    public void PaletteCollectionWithSingleColor()
    {
        var palette = new PaletteCollection
        {
            Colors.Red,
        };

        _ = Assert.Single(palette);
        Assert.Equal(palette.GetColor(0, "item"), Colors.Red);
        Assert.Equal(palette.GetColor(1, "item"), Colors.Red);
        Assert.Equal(palette.GetColor(2, "item"), Colors.Red);
    }

    [Fact]
    public void PaletteCollectionWithMultipleColors()
    {
        var palette = new PaletteCollection
        {
            Colors.Red,
            Colors.Green,
        };

        Assert.Equal(2, palette.Count);
        Assert.Equal(palette.GetColor(0, "item"), Colors.Red);
        Assert.Equal(palette.GetColor(1, "item2"), Colors.Green);
        Assert.Equal(palette.GetColor(2, "item3"), Colors.Red);
        Assert.Equal(palette.GetColor(3, "item4"), Colors.Green);
    }

    [Fact]
    public void PaletteCollectionThreeColors_CyclesCorrectly()
    {
        var palette = new PaletteCollection
        {
            Colors.Red,
            Colors.Green,
            Colors.Blue,
        };

        Assert.Equal(Colors.Red, palette.GetColor(0, "a"));
        Assert.Equal(Colors.Green, palette.GetColor(1, "b"));
        Assert.Equal(Colors.Blue, palette.GetColor(2, "c"));
        Assert.Equal(Colors.Red, palette.GetColor(3, "d"));
        Assert.Equal(Colors.Green, palette.GetColor(4, "e"));
        Assert.Equal(Colors.Blue, palette.GetColor(5, "f"));
    }

    [Fact]
    public void GetColor_LargeIndex_WrapsAroundMultipleTimes()
    {
        var palette = new PaletteCollection
        {
            Colors.Red,
            Colors.Green,
        };

        // 100 % 2 == 0 → Red; 101 % 2 == 1 → Green
        Assert.Equal(Colors.Red, palette.GetColor(100, "item"));
        Assert.Equal(Colors.Green, palette.GetColor(101, "item"));
    }

    [Fact]
    public void GetColor_ItemParameterIsNotUsed()
    {
        var palette = new PaletteCollection { Colors.Blue };

        Assert.Equal(Colors.Blue, palette.GetColor(0, null!));
        Assert.Equal(Colors.Blue, palette.GetColor(0, new object()));
        Assert.Equal(Colors.Blue, palette.GetColor(0, "any string"));
    }
}
