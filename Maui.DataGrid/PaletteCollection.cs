namespace Maui.DataGrid;

/// <summary>
/// Creates PaletteCollection for row's visual. It repeats colors consecutively for rows.
/// </summary>
public sealed class PaletteCollection : List<Color>, IColorProvider
{
    /// <summary>
    /// Determines the <c>Color</c> for the row
    /// </summary>
    /// <param name="rowIndex">Index of the row based on DataSource</param>
    /// <param name="item">Item on the index</param>
    /// <returns>Color for the row</returns>
    public Color GetColor(int rowIndex, object item)
    {
        if (this.Count > 0)
        {
            return this.ElementAt(rowIndex % this.Count);
        }

        return Colors.White;
    }
}
