namespace Maui.DataGrid.Extensions;

using System.Runtime.CompilerServices;

internal static class LayoutOptionsExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TextAlignment ToTextAlignment(this LayoutOptions layoutAlignment) => layoutAlignment.Alignment switch
    {
        LayoutAlignment.Start => TextAlignment.Start,
        LayoutAlignment.End => TextAlignment.End,
        _ => TextAlignment.Center,
    };
}
