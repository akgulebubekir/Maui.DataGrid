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

        item = default;
        return false;
    }

    public static void AddOrUpdate(this ColumnDefinitionCollection columnDefinitions, ColumnDefinition columnDefinition, int columnIndex)
    {
        if (columnIndex > columnDefinitions.Count - 1)
        {
            columnDefinitions.Add(columnDefinition);
        }
        else if (columnDefinitions[columnIndex] != columnDefinition)
        {
            columnDefinitions[columnIndex] = columnDefinition;
        }
    }

    public static void RemoveAfter(this ColumnDefinitionCollection columnDefinitions, int lastColumnIndex)
    {
        for (var i = columnDefinitions.Count - 1; i > lastColumnIndex - 1; i--)
        {
            columnDefinitions.RemoveAt(i);
        }
    }
}
