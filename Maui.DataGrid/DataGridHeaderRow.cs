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

    private Thickness _headerCellPadding = new(0, 0, 4, 0);

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

        Children.Clear(); // TODO: Use ObservableRangeCollection instead?

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

            col.HeaderCell.UpdateBindings(DataGrid, DataGrid.HeaderBordersVisible);

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

            foreach (var column in DataGrid.Columns)
            {
                column.VisibilityChanged -= OnVisibilityChanged;
            }
        }
        else
        {
            DataGrid.Columns.CollectionChanged += OnColumnsChanged;

            foreach (var column in DataGrid.Columns)
            {
                column.VisibilityChanged += OnVisibilityChanged;
            }
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

    private DataGridCell CreateHeaderCell(DataGridColumn column)
    {
        Grid cellContent;

        column.HeaderLabel.Style = column.HeaderLabelStyle ?? DataGrid.HeaderLabelStyle ?? DataGrid.DefaultHeaderStyle;

        if (!DataGrid.IsSortable || !column.SortingEnabled || !column.IsSortable())
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
                        Command = new Command<DataGridColumn>(c =>
                        {
                            ArgumentNullException.ThrowIfNull(c.DataGrid);

                            // This is to invert SortOrder when the user taps on a column.
                            var order = c.SortingOrder == SortingOrder.Ascendant
                                ? SortingOrder.Descendant
                                : SortingOrder.Ascendant;

                            var index = c.DataGrid.Columns.IndexOf(c);

                            c.DataGrid.SortedColumnIndex = new(index, order);

                            c.SortingOrder = order;
                        }, c => c.SortingEnabled && DataGrid.Columns.Contains(c)),
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
