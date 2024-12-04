namespace Maui.DataGrid.Extensions;

internal static class LayoutOptionsExtensions
{
    internal static TextAlignment ToTextAlignment(this LayoutOptions layoutAlignment) => layoutAlignment.Alignment switch
    {
        LayoutAlignment.Start => TextAlignment.Start,
        LayoutAlignment.End => TextAlignment.End,
        _ => TextAlignment.Center,
    };
}
