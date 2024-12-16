namespace Maui.DataGrid;

using System.Diagnostics.CodeAnalysis;
using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls;

[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via XAML")]
internal sealed class DataGridHeaderRow : Grid
{
    #region Bindable Properties

    public static readonly BindableProperty DataGridProperty =
        BindablePropertyExtensions.Create<DataGridHeaderRow, DataGrid>(null, BindingMode.OneTime);

    #endregion Bindable Properties

    #region Fields

    private readonly RowDefinitionCollection _headerRowDefinitions =
                [
                    new() { Height = new(1, GridUnitType.Star) },
                    new() { Height = new(1, GridUnitType.Auto) },
                ];

    private readonly Command<DataGridColumn> _sortCommand = new(OnSort, CanSort);

    #endregion Fields

    #region Properties

    public DataGrid DataGrid
    {
        get => (DataGrid)GetValue(DataGridProperty);
        set => SetValue(DataGridProperty, value);
    }

    #endregion Properties

    #region Methods

    internal void InitializeHeaderRow(bool force = false)
    {
        if (!DataGrid.IsLoaded && !force)
        {
            return;
        }

        Children.Clear();

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

            col.HeaderCell = CreateHeaderCell(col);

            col.HeaderCell.UpdateBindings(DataGrid);

            if (!col.IsVisible)
            {
                continue;
            }

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

#if NET9_0_OR_GREATER
            SetBinding(BackgroundColorProperty, BindingBase.Create<DataGrid, Color>(static x => x.BorderColor, source: DataGrid));
#else
            SetBinding(BackgroundColorProperty, new Binding(nameof(DataGrid.BorderColor), source: DataGrid));
#endif
        }
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
        if (column.HeaderCell != null)
        {
            SetFilterRow(column);

            return column.HeaderCell;
        }

        var cellContent = new Grid
        {
            RowDefinitions = _headerRowDefinitions,
        };

        column.HeaderLabel.Style = column.HeaderLabelStyle ?? DataGrid.HeaderLabelStyle ?? DataGrid.DefaultHeaderLabelStyle;
        column.FilterTextbox.Style = column.HeaderFilterStyle ?? DataGrid.HeaderFilterStyle ?? DataGrid.DefaultHeaderFilterStyle;

        column.HeaderLabelContainer.Children.Add(column.HeaderLabel);

        /* Configure the sorting icon */

        var sortIconSize = DataGrid.HeaderHeight * 0.3;
        column.SortingIconContainer.HeightRequest = sortIconSize;
        column.SortingIconContainer.WidthRequest = sortIconSize;
        column.SortingIcon.Style = DataGrid.SortIconStyle ?? DataGrid.DefaultSortIconStyle;

        column.HeaderLabelContainer.Children.Add(column.SortingIconContainer);
        column.HeaderLabelContainer.SetColumn(column.SortingIconContainer, 1);

        column.HeaderLabelContainer.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = _sortCommand,
            CommandParameter = column,
        });

        cellContent.Children.Add(column.HeaderLabelContainer);

        SetFilterRow(column);

        cellContent.Children.Add(column.FilterTextboxContainer);
        cellContent.SetRow(column.FilterTextboxContainer, 1);
        cellContent.SetColumnSpan(column.FilterTextboxContainer, 2);

        return new DataGridCell(cellContent, DataGrid.HeaderBackground, column, false);
    }

    private void SetFilterRow(DataGridColumn column)
    {
        if (DataGrid.FilteringEnabled && column.FilteringEnabled)
        {
            column.FilterTextboxContainer.Content = column.FilterTextbox;
        }
        else if (DataGrid.FilteringEnabled && DataGrid.Columns.Any(c => c.FilteringEnabled))
        {
            // Add placeholder
            column.FilterTextboxContainer.Content = new Entry
            {
                Style = column.FilterTextbox.Style,
                IsEnabled = false,
            };
        }
        else
        {
            column.FilterTextboxContainer.Content = null;
        }
    }

    #endregion Methods
}
