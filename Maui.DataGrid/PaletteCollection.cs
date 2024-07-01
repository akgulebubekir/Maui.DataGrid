namespace Maui.DataGrid;

/// <summary>
/// Creates PaletteCollection for row's visual. It repeats colors consecutively for rows.
/// </summary>
public sealed class PaletteCollection : List<Color>, IColorProvider
{
    /// <summary>
    /// Determines the <see cref="Color"/> for the row.
    /// </summary>
    /// <param name="rowIndex">Index of the row based on DataSource.</param>
    /// <param name="item">Item on the index.</param>
    /// <returns>Color for the row.</returns>
    public Color GetColor(int rowIndex, object item)
    {
        if (Count > 0)
        {
            return this[rowIndex % Count];
        }

        return Colors.White;
    }
}
