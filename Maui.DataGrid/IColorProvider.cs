namespace Maui.DataGrid;

public interface IColorProvider
{
    Color GetColor(int rowIndex, object item);
}