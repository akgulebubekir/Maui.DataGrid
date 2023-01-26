namespace Maui.DataGrid;

using Maui.DataGrid.Utils;

internal class DataGridRowTemplateSelector : DataTemplateSelector
{
    private static DataTemplate _dataGridRowTemplate;

    public DataGridRowTemplateSelector()
    {
        _dataGridRowTemplate = new DataTemplate(typeof(DataGridRow));
    }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var collectionView = (CollectionView)container;
        var dataGrid = (DataGrid)collectionView.Parent.Parent;
        var items = dataGrid.InternalItems;

        _dataGridRowTemplate.SetValueIfNeeded(DataGridRow.DataGridProperty, dataGrid);
        _dataGridRowTemplate.SetValueIfNeeded(DataGridRow.RowContextProperty, item);
        _dataGridRowTemplate.SetValueIfNeeded(VisualElement.HeightRequestProperty, dataGrid.RowHeight);

        if (items != null)
        {
            _dataGridRowTemplate.SetValueIfNeeded(DataGridRow.IndexProperty, items.IndexOf(item));
        }

        return _dataGridRowTemplate;
    }
}