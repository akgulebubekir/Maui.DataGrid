namespace Maui.DataGrid.Utils;

internal static class LayoutOptionsExtensions
{
    internal static TextAlignment ToTextAlignment(this LayoutOptions layoutAlignment) => layoutAlignment.Alignment switch
    {
        LayoutAlignment.Start => TextAlignment.Start,
        LayoutAlignment.End => TextAlignment.End,
        _ => TextAlignment.Center,
    };
}
