namespace Maui.DataGrid;

/// <summary>
/// Represents when <see cref="DataGrid.RowTappedCommand"/> is executed, and what it receives.
/// </summary>
public enum RowTappedCommandMode
{
    /// <summary>
    /// The command is executed only when the selection changes, and receives the
    /// <see cref="SelectionChangedEventArgs"/>. Re-tapping the selected row does not execute it,
    /// and neither does any tap while <see cref="DataGrid.SelectionMode"/> is
    /// <see cref="Microsoft.Maui.Controls.SelectionMode.None"/>.
    /// This is the legacy behavior, and remains the default.
    /// </summary>
    SelectionChanged = 0,

    /// <summary>
    /// The command is executed on every row tap, and receives the tapped item, regardless of
    /// <see cref="DataGrid.SelectionMode"/>. It is not executed when the selection changes without
    /// a tap, so a single tap executes it exactly once.
    /// </summary>
    Tap = 1,
}
