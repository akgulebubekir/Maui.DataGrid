namespace Maui.DataGrid.Utils;

internal static class LayoutOptionsExtensions
{
    /// <summary>
    /// Convert LayoutOptions to TextAlignment.
    /// </summary>
    /// <param name="layoutAlignment">The LayoutOptions object to convert.</param>
    /// <returns>The converted TextAlignment.</returns>
    internal static TextAlignment ToTextAlignment(this LayoutOptions layoutAlignment) => layoutAlignment.Alignment switch
    {
        LayoutAlignment.Start => TextAlignment.Start,
        LayoutAlignment.End => TextAlignment.End,
        _ => TextAlignment.Center,
    };
}
