namespace Maui.DataGrid.Sample.Tests;

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
}
