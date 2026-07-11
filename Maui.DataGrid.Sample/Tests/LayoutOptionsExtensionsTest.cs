namespace Maui.DataGrid.Sample.Tests;

using Maui.DataGrid.Extensions;
using Xunit;

public class LayoutOptionsExtensionsTest
{
    [Fact]
    public void StartLayoutReturnsStartTextAlignment()
    {
        var result = LayoutOptions.Start.ToTextAlignment();

        Assert.Equal(TextAlignment.Start, result);
    }

    [Fact]
    public void EndLayoutReturnsEndTextAlignment()
    {
        var result = LayoutOptions.End.ToTextAlignment();

        Assert.Equal(TextAlignment.End, result);
    }

    [Fact]
    public void CenterLayoutReturnsCenterTextAlignment()
    {
        var result = LayoutOptions.Center.ToTextAlignment();

        Assert.Equal(TextAlignment.Center, result);
    }

    [Fact]
    public void FillLayoutReturnsCenterTextAlignment()
    {
        var result = LayoutOptions.Fill.ToTextAlignment();

        Assert.Equal(TextAlignment.Center, result);
    }
}
