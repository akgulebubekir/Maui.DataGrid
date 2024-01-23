namespace Maui.DataGrid.Extensions;

internal static class ListExtensions
{
    public static bool TryGetItem<T>(this IList<T> list, int index, out T? item)
    {
        if (index >= 0 && index < list.Count)
        {
            item = list[index];
            return true;
        }
        else
        {
            item = default;
            return false;
        }
    }
}
