namespace Maui.DataGrid;

/// <summary>
/// Keeps the header row and the data rows in column agreement.
/// <para>
/// The header and every row are separate <see cref="Grid"/>s which share one
/// <see cref="ColumnDefinition"/> per column, but a shared definition is only a shared *instruction*:
/// each Grid resolves it against its own available width and its own content. That is what makes
/// columns drift apart in two ways. The header lays out over the full width of the control while the
/// rows lay out inside the items host, which on some platforms is narrower by the width of a vertical
/// scrollbar, so <c>Star</c> columns resolve wider in the header than in the rows and the error
/// accumulates left to right (#188). And an <c>Auto</c> column is measured against the content of one
/// Grid only, so every row picks its own width for it (#118).
/// </para>
/// <para>
/// Both are fixed here by removing the disagreement at its source: the header reserves the width the
/// rows do not get, and an <c>Auto</c> column is measured across the header and every realized row and
/// then pinned to one absolute width, which resolves identically everywhere.
/// </para>
/// </summary>
internal sealed class ColumnWidthCoordinator(DataGrid dataGrid)
{
    #region Fields

    /// <summary>
    /// Layout differences below this many device-independent units are rounding noise rather than real
    /// disagreement, and acting on them would only trigger another layout pass.
    /// </summary>
    private const double Tolerance = 0.5;

    /// <summary>
    /// The rows which are currently realized. The CollectionView materializes only the rows it needs
    /// and recycles them, so this is the set of rows an <c>Auto</c> column has to fit.
    /// </summary>
    private readonly HashSet<DataGridRow> _rows = [];

    private bool _autoWidthUpdatePending;

    #endregion Fields

    #region Properties

    internal IReadOnlyCollection<DataGridRow> Rows => _rows;

    /// <summary>
    /// Gets the number of <c>Auto</c> width passes which have run. A pass changes nothing until the
    /// platform can measure a cell, so this is how a headless test observes that one was scheduled, and
    /// that grids without <c>Auto</c> columns never schedule one at all.
    /// </summary>
    internal int AutoWidthPassCount { get; private set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Pins a column to an absolute width, which is the whole point of the exercise: an absolute width
    /// resolves to the same number in the header's Grid and in every row's Grid, whereas <c>Auto</c>
    /// resolves against whatever content one Grid happens to hold. The column's public
    /// <see cref="DataGridColumn.Width"/> stays <c>Auto</c>, so a consumer reading it back still sees
    /// what they asked for, and changing it un-pins the column.
    /// </summary>
    /// <param name="column">The column to pin.</param>
    /// <param name="desiredWidth">The width of the column's widest cell.</param>
    internal static void ApplySharedWidth(DataGridColumn column, double desiredWidth)
    {
        ArgumentNullException.ThrowIfNull(column);

        // Nothing measured, which is a column whose cells have not been laid out yet rather than a
        // column which wants to be invisible. Leaving it Auto lets it size itself in the meantime.
        if (!IsUsableWidth(desiredWidth))
        {
            return;
        }

        var columnDefinition = column.ColumnDefinition;

        if (columnDefinition == null)
        {
            return;
        }

        var current = columnDefinition.Width;

        if (current.IsAbsolute && Math.Abs(current.Value - desiredWidth) < Tolerance)
        {
            return;
        }

        columnDefinition.Width = new GridLength(desiredWidth);
    }

    internal void AddRow(DataGridRow row)
    {
        if (_rows.Add(row))
        {
            InvalidateAutoWidths();
        }
    }

    internal void RemoveRow(DataGridRow row)
    {
        if (!_rows.Remove(row))
        {
            return;
        }

        if (_rows.Count == 0)
        {
            // Without a row to compare against there is nothing to reserve, and a stale reservation
            // would leave the header of an emptied grid short.
            ReserveHeaderWidth(0);
        }

        InvalidateAutoWidths();
    }

    /// <summary>
    /// Reacts to a row being laid out. A row fills the items host, so its width is the width the header
    /// has to match.
    /// </summary>
    /// <param name="rowWidth">The width the row was given.</param>
    internal void OnRowWidthChanged(double rowWidth)
    {
        ReserveHeaderWidth(dataGrid.ItemsHostWidth, rowWidth);

        // A row which has been given a size is a row whose cells the platform can finally measure, so
        // this is the earliest point at which an Auto column can learn what it has to fit.
        InvalidateAutoWidths();
    }

    /// <summary>
    /// Reserves, as right padding on the header, the width which the rows do not get. The header then
    /// resolves its columns over the same width as the rows do, which is what keeps the two aligned.
    /// </summary>
    /// <param name="itemsHostWidth">The width of the control which hosts the rows, scrollbar included.</param>
    /// <param name="rowWidth">The width a realized row was given, scrollbar excluded.</param>
    internal void ReserveHeaderWidth(double itemsHostWidth, double rowWidth)
    {
        // Nothing has been laid out yet, so there is no reservation to deduce.
        if (!IsUsableWidth(itemsHostWidth) || !IsUsableWidth(rowWidth))
        {
            return;
        }

        var reserved = itemsHostWidth - rowWidth;

        // A scrollbar is a sliver of the control. Anything larger means the row is not filling the
        // items host at all, and reserving that much would push the header's columns badly out of shape.
        if (reserved < Tolerance || reserved > itemsHostWidth / 2)
        {
            reserved = 0;
        }

        ReserveHeaderWidth(reserved);
    }

    /// <summary>
    /// Schedules a pass over the <c>Auto</c> columns. Recycling a row invalidates measurements far more
    /// often than they can usefully be recomputed, so passes are coalesced into one per dispatcher turn.
    /// </summary>
    internal void InvalidateAutoWidths()
    {
        if (_autoWidthUpdatePending || !HasAutoColumns())
        {
            return;
        }

        _autoWidthUpdatePending = true;

        _ = dataGrid.Dispatcher.Dispatch(UpdateAutoWidths);
    }

    /// <summary>
    /// Measures each <c>Auto</c> column across the header and every realized row, and pins it to the
    /// width of its widest cell.
    /// </summary>
    internal void UpdateAutoWidths()
    {
        try
        {
            AutoWidthPassCount++;

            var columns = dataGrid.Columns;

            if (columns == null)
            {
                return;
            }

            foreach (var column in columns)
            {
                if (column.IsVisible && column.Width.IsAuto)
                {
                    ApplySharedWidth(column, MeasureWidestCell(column));
                }
            }
        }
        finally
        {
            // Cleared last, so that the measure invalidations this pass causes by pinning a column are
            // absorbed rather than scheduling a pass which would measure exactly the same content.
            _autoWidthUpdatePending = false;
        }
    }

    private static bool IsUsableWidth(double width) => !double.IsNaN(width) && !double.IsInfinity(width) && width > 0;

    /// <summary>
    /// Measures a cell against unbounded width, so that the result is the width the cell's content
    /// wants rather than the width it was last given. Measuring against the allocated width instead
    /// would let a pinned column hold itself at whatever width it already had.
    /// </summary>
    private static double MeasureContentWidth(DataGridCell? cell) =>
        cell == null ? 0 : ((IView)cell).Measure(double.PositiveInfinity, double.PositiveInfinity).Width;

    private double MeasureWidestCell(DataGridColumn column)
    {
        var widest = MeasureContentWidth(column.HeaderCell);

        foreach (var row in _rows)
        {
            // A row the CollectionView has recycled out of view holds no item, and its cells therefore
            // show nothing that needs fitting.
            if (row.BindingContext != null)
            {
                widest = Math.Max(widest, MeasureContentWidth(row.GetCellForColumn(column)));
            }
        }

        return widest;
    }

    private bool HasAutoColumns()
    {
        var columns = dataGrid.Columns;

        if (columns == null)
        {
            return false;
        }

        foreach (var column in columns)
        {
            if (column.IsVisible && column.Width.IsAuto)
            {
                return true;
            }
        }

        return false;
    }

    private void ReserveHeaderWidth(double reserved)
    {
        var headerRow = dataGrid.HeaderRow;

        var padding = headerRow.Padding;

        if (Math.Abs(padding.Right - reserved) < Tolerance)
        {
            return;
        }

        headerRow.Padding = new Thickness(padding.Left, padding.Top, reserved, padding.Bottom);
    }

    #endregion Methods
}
