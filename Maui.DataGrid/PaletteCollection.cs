namespace Maui.DataGrid;

public sealed class PaletteCollection : List<Color>, IColorProvider
{
    public Color GetColor(int rowIndex, object item)
    {
        return Count > 0 ? this.ElementAt(rowIndex % Count) : default;
    }
}