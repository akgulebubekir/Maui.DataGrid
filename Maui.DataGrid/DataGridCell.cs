namespace Maui.DataGrid;

using Microsoft.Maui.Controls;

/// <summary>
/// Specifies each cell of the DataGrid.
/// </summary>
internal sealed class DataGridCell : ContentView
{
    internal DataGridCell(View cellContent, Color? backgroundColor, DataGridColumn column, bool isEditing)
    {
        Content = new ContentView
        {
            BackgroundColor = backgroundColor,
            Content = cellContent,
        };

        Column = column;
        IsEditing = isEditing;
    }

    public DataGridColumn Column { get; }

    public bool IsEditing { get; }

    internal void UpdateCellBackgroundColor(Color? bgColor)
    {
        foreach (var child in Children)
        {
            if (child is ContentView cellContent)
            {
                cellContent.BackgroundColor = bgColor;
            }
        }
    }

    internal void UpdateCellTextColor(Color? textColor)
    {
        foreach (var child in Children)
        {
            if (child is ContentView cellContent && cellContent.Content is Label label)
            {
                label.TextColor = textColor;
            }
        }
    }
}
