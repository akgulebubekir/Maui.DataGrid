namespace Maui.DataGrid;

using Microsoft.Maui.Controls;

/// <summary>
/// Specifies each cell of the DataGrid.
/// </summary>
internal sealed class DataGridCell : Grid
{
    internal DataGridCell(View cellContent, Color? backgroundColor, DataGridColumn column, bool isEditing)

    {
        var colorfulCellContent = new ContentView
        {
            BackgroundColor = backgroundColor,
            Content = cellContent,
        };

        Content = cellContent;
        Column = column;
        IsEditing = isEditing;

        Children.Add(colorfulCellContent);
    }

    public View Content { get; }

    public DataGridColumn Column { get; }
    public bool IsEditing { get; }


    internal void UpdateBindings(DataGrid dataGrid, bool bordersVisible = true)
    {
        // The DataGridCell is a grid, and the padding constitutes the cell's border
        // And the Background constitutes the border color of the cell.

        if (bordersVisible)
        {
            BackgroundColor = dataGrid.BorderColor;
            Padding = dataGrid.BorderThickness;

            SetBinding(BackgroundColorProperty, new Binding(nameof(DataGrid.BorderColor), source: dataGrid));
            SetBinding(PaddingProperty, new Binding(nameof(DataGrid.BorderThickness), source: dataGrid));
        }
        else
        {
            Padding = 0;

            RemoveBinding(BackgroundColorProperty);
            RemoveBinding(PaddingProperty);
        }
    }

    internal void UpdateCellColors(Color? bgColor, Color? textColor = null)
    {
        foreach (var cellContent in Children.OfType<ContentView>())
        {
            cellContent.BackgroundColor = bgColor;

            if (cellContent.Content is Label label && textColor != null)
            {
                label.TextColor = textColor;
            }
        }
    }
}
