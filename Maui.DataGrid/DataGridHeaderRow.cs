namespace Maui.DataGrid;

using Extensions;
using Microsoft.Maui.Controls;

internal sealed class DataGridHeaderRow : Grid
{
    #region Fields

    private readonly ColumnDefinitionCollection _headerColumnDefinitions =
                [
                    new() { Width = new(1, GridUnitType.Star) },
                    new() { Width = new(1, GridUnitType.Auto) }
                ];

    private readonly Thickness _headerCellPadding = new(0, 0, 4, 0);

    private readonly Command<DataGridColumn> _sortCommand = new(OnSort, CanSort);

    #endregion Fields

    #region Properties

    public DataGrid DataGrid
    {
        get => (DataGrid)GetValue(DataGridProperty);
        set => SetValue(DataGridProperty, value);
    }

    #endregion Properties

    #region Bindable Properties

    public static readonly BindableProperty DataGridProperty =
        BindablePropertyExtensions.Create<DataGridHeaderRow, DataGrid>(null, BindingMode.OneTime);

    #endregion Bindable Properties

    #region Methods

    internal void InitializeHeaderRow(bool force = false)
    {
        if (!DataGrid.IsLoaded && !force)
        {
            return;
        }

        Children.Clear();

        UpdateBorders();

        if (DataGrid.Columns == null || DataGrid.Columns.Count == 0)
        {
            ColumnDefinitions.Clear();
            return;
        }

        var columnCount = DataGrid.Columns.Count;

        for (var i = 0; i < columnCount; i++)
        {
            var col = DataGrid.Columns[i];

            col.DataGrid ??= DataGrid;

            col.BindingContext ??= BindingContext;

            col.InitializeDataType();

            col.ColumnDefinition ??= new(col.Width);

            // Add or update columns as needed
            ColumnDefinitions.AddOrUpdate(col.ColumnDefinition, i);

            if (!col.IsVisible)
            {
                continue;
            }

            col.HeaderCell ??= CreateHeaderCell(col);

            if (Children.TryGetItem(i, out var existingChild))
            {
                if (existingChild is not DataGridCell existingCell)
                {
                    throw new InvalidDataException($"Header row should only contain {nameof(DataGridCell)}s");
                }

                if (existingCell.Column != col)
                {
                    this.SetColumn(col.HeaderCell, i);
                    Children[i] = col.HeaderCell;
                }
            }
            else
            {
                this.SetColumn(col.HeaderCell, i);
                Children.Add(col.HeaderCell);
            }
        }

        // Remove extra columns, if any
        ColumnDefinitions.RemoveAfter(DataGrid.Columns.Count);
    }

    private void UpdateBorders()
    {
        // This approach is a hack to avoid needing a slow Border control.
        // The padding constitutes the cell's border thickness.
        // And the BackgroundColor constitutes the border color of the cell.
        if (DataGrid.HeaderBordersVisible)
        {
            var borderSize = DataGrid.BorderThickness;
            ColumnSpacing = borderSize.Left;
            Padding = new(0, borderSize.Top / 2, 0, borderSize.Bottom / 2);
        }
        else
        {
            ColumnSpacing = 0;
            Padding = 0;
        }
    }

    private void OnBorderThicknessChanged(object? sender, EventArgs e)
    {
        UpdateBorders();
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        InitializeHeaderRow();
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent == null)
        {
            DataGrid.Columns.CollectionChanged -= OnColumnsChanged;
            DataGrid.BorderThicknessChanged -= OnBorderThicknessChanged;

            foreach (var column in DataGrid.Columns)
            {
                column.VisibilityChanged -= OnVisibilityChanged;
            }
        }
        else
        {
            DataGrid.Columns.CollectionChanged += OnColumnsChanged;
            DataGrid.BorderThicknessChanged += OnBorderThicknessChanged;

            foreach (var column in DataGrid.Columns)
            {
                column.VisibilityChanged += OnVisibilityChanged;
            }

            SetBinding(BackgroundColorProperty, new Binding(nameof(DataGrid.BorderColor), source: DataGrid));
        }
    }

    private void OnColumnsChanged(object? sender, EventArgs e)
    {
        InitializeHeaderRow();
    }

    private void OnVisibilityChanged(object? sender, EventArgs e)
    {
        InitializeHeaderRow();
    }

    private static void OnSort(DataGridColumn column)
    {
        ArgumentNullException.ThrowIfNull(column.DataGrid);

        // This is to invert SortOrder when the user taps on a column.
        var order = column.SortingOrder == SortingOrder.Ascendant
            ? SortingOrder.Descendant
            : SortingOrder.Ascendant;

        var index = column.DataGrid.Columns.IndexOf(column);

        // This actually does the sorting, via the propertyChanged event.
        column.DataGrid.SortedColumnIndex = new(index, order);

        column.SortingOrder = order;
    }

    private static bool CanSort(DataGridColumn column)
    {
        ArgumentNullException.ThrowIfNull(column.DataGrid);

        return column.SortingEnabled && column.DataGrid.Columns.Contains(column);
    }

    private DataGridCell CreateHeaderCell(DataGridColumn column)
    {
        Grid cellContent;

        column.HeaderLabel.Style = column.HeaderLabelStyle ?? DataGrid.HeaderLabelStyle ?? DataGrid.DefaultHeaderStyle;

        if (!DataGrid.SortingEnabled || !column.SortingEnabled || !column.IsSortable())
        {
            cellContent = [column.HeaderLabel];
            cellContent.Padding = _headerCellPadding;
        }
        else
        {
            var sortIconSize = DataGrid.HeaderHeight * 0.3;
            column.SortingIconContainer.HeightRequest = sortIconSize;
            column.SortingIconContainer.WidthRequest = sortIconSize;
            column.SortingIcon.Style = DataGrid.SortIconStyle ?? DataGrid.DefaultSortIconStyle;

            cellContent = new Grid
            {
                Padding = _headerCellPadding,
                ColumnDefinitions = _headerColumnDefinitions,
                Children = { column.HeaderLabel, column.SortingIconContainer },
                GestureRecognizers =
                {
                    new TapGestureRecognizer
                    {
                        Command = _sortCommand,
                        CommandParameter = column
                    }
                }
            };

            cellContent.SetColumn(column.SortingIconContainer, 1);
        }

        return new DataGridCell(cellContent, DataGrid.HeaderBackground, column, false);
    }

    #endregion Methods
}
