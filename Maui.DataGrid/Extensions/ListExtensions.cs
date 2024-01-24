namespace Maui.DataGrid.Extensions;

internal static class ListExtensions
{
    public static bool TryGetItem(this IList<IView> list, int index, out IView? item)
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
