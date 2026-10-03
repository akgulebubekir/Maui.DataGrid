namespace Maui.DataGrid;

using Maui.DataGrid.Converters;
using Microsoft.Maui.Controls;

/// <summary>
/// Specifies each cell of the DataGrid.
/// </summary>
internal sealed class DataGridCell : ContentView
{
    internal DataGridCell(View cellContent, Color? backgroundColor, DataGridColumn column, bool isEditing, DataTemplate? contentTemplate = null)
    {
        Content = new ContentView
        {
            BackgroundColor = backgroundColor,
            Content = cellContent,
        };

        Column = column;
        IsEditing = isEditing;
        ContentTemplate = contentTemplate;
    }

    public DataGridColumn Column { get; }

    public bool IsEditing { get; }

    /// <summary>
    /// Gets the template the content was created from, with any <see cref="DataTemplateSelector"/>
    /// already resolved, or null when the content was not created from a template. A recycled row
    /// compares this against a fresh selection to notice that its new item needs another template.
    /// </summary>
    public DataTemplate? ContentTemplate { get; }

    internal void UpdateBindings(DataGrid dataGrid)
    {
        // This approach is a hack to avoid needing a slow Border control.
        // The padding constitutes the cell's border thickness.
        // And the BackgroundColor constitutes the border color of the cell.
        if (dataGrid.HeaderBordersVisible)
        {
#if NET9_0_OR_GREATER
            SetBinding(BackgroundColorProperty, BindingBase.Create<DataGrid, Color>(static x => x.BorderBackingColor, source: dataGrid));
            SetBinding(PaddingProperty, BindingBase.Create<DataGrid, Thickness>(static x => x.BorderThickness, converter: new BorderThicknessToCellPaddingConverter(), source: dataGrid));
#else
            SetBinding(BackgroundColorProperty, new Binding(nameof(DataGrid.BorderBackingColor), source: dataGrid));
            SetBinding(PaddingProperty, new Binding(nameof(DataGrid.BorderThickness), converter: new BorderThicknessToCellPaddingConverter(), source: dataGrid));
#endif
        }
        else
        {
            RemoveBinding(BackgroundColorProperty);
            RemoveBinding(PaddingProperty);

            Padding = 0;

            // Removing the binding leaves the last value it wrote in place, and with no padding to fill
            // that colour can only ever bleed through the gaps between cells.
            BackgroundColor = Colors.Transparent;
        }
    }

    internal void UpdateCellBackgroundColor(Color? bgColor)
    {
        foreach (var child in ((IVisualTreeElement)this).GetVisualChildren())
        {
            if (child is ContentView cellContent)
            {
                cellContent.BackgroundColor = bgColor;
            }
        }
    }

    internal void UpdateCellTextColor(Color? textColor)
    {
        foreach (var child in ((IVisualTreeElement)this).GetVisualChildren())
        {
            if (child is ContentView cellContent && cellContent.Content is Label label)
            {
                label.TextColor = textColor;
            }
        }
    }
}
