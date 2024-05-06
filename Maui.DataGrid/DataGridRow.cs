namespace Maui.DataGrid;

using Extensions;
using Microsoft.Maui.Controls;

internal sealed class DataGridRow : Grid
{
    #region Fields

    private bool _wasSelected;

    #endregion Fields

    #region Bindable Properties

    /// <summary>
    /// Gets or sets the background color of the cells within this DataGridRow.
    /// </summary>
    public static readonly BindableProperty CellBackgroundColorProperty =
        BindablePropertyExtensions.Create<DataGridRow, Color>(Colors.White,
            propertyChanged: (b, o, n) =>
            {
                if (b is not DataGridRow self)
                {
                    return;
                }

                foreach (var child in self.Children)
                {
                    if (child is DataGridCell cell)
                    {
                        cell.UpdateCellBackgroundColor(n);
                    }
                }
            });

    /// <summary>
    /// Gets or sets the text color of the cells within this DataGridRow.
    /// </summary>
    public static readonly BindableProperty CellTextColorProperty =
        BindablePropertyExtensions.Create<DataGridRow, Color>(Colors.White,
            propertyChanged: (b, o, n) =>
            {
                if (b is not DataGridRow self)
                {
                    return;
                }

                foreach (var child in self.Children)
                {
                    if (child is DataGridCell cell)
                    {
                        cell.UpdateCellTextColor(n);
                    }
                }
            });

    #endregion Bindable Properties

    #region Properties

    public DataGrid DataGrid
    {
        get => (DataGrid)GetValue(DataGridProperty);
        set => SetValue(DataGridProperty, value);
    }

    public object RowToEdit
    {
        get => GetValue(RowToEditProperty);
        set => SetValue(RowToEditProperty, value);
    }

    public Color CellBackgroundColor
    {
        get => (Color)GetValue(CellBackgroundColorProperty);
        set => SetValue(CellBackgroundColorProperty, value);
    }

    public Color CellTextColor
    {
        get => (Color)GetValue(CellTextColorProperty);
        set => SetValue(CellTextColorProperty, value);
    }

    #endregion Properties

    #region Bindable Properties

    public static readonly BindableProperty DataGridProperty =
        BindablePropertyExtensions.Create<DataGridRow, DataGrid>(null, BindingMode.OneTime);

    public static readonly BindableProperty RowToEditProperty =
        BindablePropertyExtensions.Create<DataGridRow, object>(null, BindingMode.OneWay,
            propertyChanged: (b, o, n) =>
            {
                if (b is not DataGridRow row)
                {
                    return;
                }

                if (o == row.BindingContext || n == row.BindingContext)
                {
                    row.InitializeRow();
                }
            });

    #endregion Bindable Properties

    #region Methods

    private void InitializeRow()
    {
        Children.Clear(); // TODO: Revisit this if and when virtualization is straightened out

        UpdateSelectedState();

        UpdateColors();

        UpdateBorders();

        var columns = DataGrid.Columns;

        if (columns == null || columns.Count == 0)
        {
            ColumnDefinitions.Clear();
            return;
        }

        var isEditing = RowToEdit == BindingContext;

        var columnCount = columns.Count;

        for (var i = 0; i < columnCount; i++)
        {
            var col = columns[i];

            if (col.ColumnDefinition == null)
            {
                continue;
            }

            // Add or update columns as needed
            ColumnDefinitions.AddOrUpdate(col.ColumnDefinition, i);

            if (!col.IsVisible)
            {
                continue;
            }

            if (Children.TryGetItem(i, out var existingChild))
            {
                if (existingChild is not DataGridCell existingCell)
                {
                    throw new InvalidDataException($"{nameof(DataGridRow)} should only contain {nameof(DataGridCell)}s");
                }

                if (existingCell.Column != col || existingCell.IsEditing != isEditing)
                {
                    Children[i] = GenerateCellForColumn(col, i);
                }
            }
            else
            {
                var newCell = GenerateCellForColumn(col, i);
                Children.Add(newCell);
            }
        }

        // Remove extra columns, if any
        ColumnDefinitions.RemoveAfter(columnCount);
    }

    private DataGridCell GenerateCellForColumn(DataGridColumn col, int columnIndex)
    {
        var dataGridCell = CreateCell(col);

        SetColumn((BindableObject)dataGridCell, columnIndex);

        return dataGridCell;
    }

    private DataGridCell CreateCell(DataGridColumn col)
    {
        View cellContent;

        var isEditing = RowToEdit == BindingContext;

        if (isEditing)
        {
            cellContent = CreateEditCell(col);
        }
        else
        {
            cellContent = CreateViewCell(col);
        }

        return new DataGridCell(cellContent, CellBackgroundColor, col, isEditing);
    }

    private View CreateViewCell(DataGridColumn col)
    {
        View cell;

        if (col.CellTemplate != null)
        {
            cell = (View)col.CellTemplate.CreateContent();

            if (!string.IsNullOrWhiteSpace(col.PropertyName))
            {
                cell.SetBinding(BindingContextProperty,
                    new Binding(col.PropertyName, source: BindingContext));
            }
        }
        else
        {
            cell = new Label
            {
                TextColor = CellTextColor,
                VerticalTextAlignment = col.VerticalTextAlignment,
                HorizontalTextAlignment = col.HorizontalTextAlignment,
                LineBreakMode = col.LineBreakMode,
                FontSize = DataGrid.FontSize,
                FontFamily = DataGrid.FontFamily
            };

            if (!string.IsNullOrWhiteSpace(col.PropertyName))
            {
                cell.SetBinding(Label.TextProperty,
                    new Binding(col.PropertyName, stringFormat: col.StringFormat, source: BindingContext));
            }
        }

        return cell;
    }

    private View CreateEditCell(DataGridColumn col)
    {
        var cell = GenerateTemplatedEditCell(col);

        return cell ?? CreateDefaultEditCell(col);
    }

    private View CreateDefaultEditCell(DataGridColumn col)
    {
        var typeCode = Type.GetTypeCode(col.DataType);

        return typeCode switch
        {
            TypeCode.String => GenerateTextEditCell(col),
            TypeCode.Boolean => GenerateBooleanEditCell(col),
            TypeCode.Decimal => GenerateNumericEditCell(col, v => decimal.TryParse(v.TrimEnd(',', '.'), out _)),
            TypeCode.Double => GenerateNumericEditCell(col, v => double.TryParse(v.TrimEnd(',', '.'), out _)),
            TypeCode.Int16 => GenerateNumericEditCell(col, v => short.TryParse(v, out _)),
            TypeCode.Int32 => GenerateNumericEditCell(col, v => int.TryParse(v, out _)),
            TypeCode.Int64 => GenerateNumericEditCell(col, v => long.TryParse(v, out _)),
            TypeCode.SByte => GenerateNumericEditCell(col, v => sbyte.TryParse(v, out _)),
            TypeCode.Single => GenerateNumericEditCell(col, v => float.TryParse(v.TrimEnd(',', '.'), out _)),
            TypeCode.UInt16 => GenerateNumericEditCell(col, v => ushort.TryParse(v, out _)),
            TypeCode.UInt32 => GenerateNumericEditCell(col, v => uint.TryParse(v, out _)),
            TypeCode.UInt64 => GenerateNumericEditCell(col, v => ulong.TryParse(v, out _)),
            TypeCode.DateTime => GenerateDateTimeEditCell(col),
            _ => new TemplatedView(),
        };
    }

    private View? GenerateTemplatedEditCell(DataGridColumn col)
    {
        if (col.EditCellTemplate == null)
        {
            return null;
        }

        var cell = (View)col.EditCellTemplate.CreateContent();

        if (!string.IsNullOrWhiteSpace(col.PropertyName))
        {
            cell.SetBinding(BindingContextProperty,
                new Binding(col.PropertyName, source: BindingContext));
        }

        return cell;
    }

    private Entry GenerateTextEditCell(DataGridColumn col)
    {
        var entry = new Entry
        {
            TextColor = CellTextColor,
            VerticalTextAlignment = col.VerticalTextAlignment,
            HorizontalTextAlignment = col.HorizontalTextAlignment,
            FontSize = DataGrid.FontSize,
            FontFamily = DataGrid.FontFamily
        };

        if (!string.IsNullOrWhiteSpace(col.PropertyName))
        {
            entry.SetBinding(Entry.TextProperty,
                new Binding(col.PropertyName, BindingMode.TwoWay, stringFormat: col.StringFormat, source: BindingContext));
        }

        return entry;
    }

    private CheckBox GenerateBooleanEditCell(DataGridColumn col)
    {
        var checkBox = new CheckBox
        {
            Color = CellTextColor,
            BackgroundColor = CellBackgroundColor,
        };

        if (!string.IsNullOrWhiteSpace(col.PropertyName))
        {
            checkBox.SetBinding(CheckBox.IsCheckedProperty,
                new Binding(col.PropertyName, BindingMode.TwoWay, source: BindingContext));
        }

        return checkBox;
    }

    private Entry GenerateNumericEditCell(DataGridColumn col, Func<string, bool> numericParser)
    {
        var entry = new Entry
        {
            TextColor = CellTextColor,
            VerticalTextAlignment = col.VerticalTextAlignment,
            HorizontalTextAlignment = col.HorizontalTextAlignment,
            FontSize = DataGrid.FontSize,
            FontFamily = DataGrid.FontFamily,
            Keyboard = Keyboard.Numeric
        };

        entry.TextChanged += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.NewTextValue) && !numericParser(e.NewTextValue))
            {
                ((Entry)s!).Text = e.OldTextValue;
            }
        };

        if (!string.IsNullOrWhiteSpace(col.PropertyName))
        {
            entry.SetBinding(Entry.TextProperty,
                new Binding(col.PropertyName, BindingMode.TwoWay, source: BindingContext));
        }

        return entry;
    }

    private DatePicker GenerateDateTimeEditCell(DataGridColumn col)
    {
        var datePicker = new DatePicker
        {
            TextColor = CellTextColor,
        };

        if (!string.IsNullOrWhiteSpace(col.PropertyName))
        {
            datePicker.SetBinding(DatePicker.DateProperty,
                new Binding(col.PropertyName, BindingMode.TwoWay, source: BindingContext));
        }

        return datePicker;
    }

    private void UpdateBorders()
    {
        // This approach is a hack to avoid needing a slow Border control.
        // The padding constitutes the cell's border thickness.
        // And the BackgroundColor constitutes the border color of the cell.
        var borderSize = DataGrid.BorderThickness;
        ColumnSpacing = borderSize.Left;
        Padding = new(0, borderSize.Top / 2, 0, borderSize.Bottom / 2);
    }

    private void UpdateColors()
    {
        var rowIndex = DataGrid.InternalItems.IndexOf(BindingContext);

        if (rowIndex == -1)
        {
            return;
        }

        var isSelected = DataGrid.SelectionMode != SelectionMode.None && _wasSelected;

        CellBackgroundColor = isSelected
                ? DataGrid.ActiveRowColor
                : DataGrid.RowsBackgroundColorPalette.GetColor(rowIndex, BindingContext);
        CellTextColor = isSelected
                ? InverseColor(DataGrid.ActiveRowColor)
                : DataGrid.RowsTextColorPalette.GetColor(rowIndex, BindingContext);
    }

    private static Color InverseColor(Color color)
    {
        var brightness = (0.299 * color.Red) + (0.587 * color.Green) + (0.114 * color.Blue);
        return brightness < 0.5 ? Colors.White : Colors.Black;
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        InitializeRow();
    }

    private void OnBorderThicknessChanged(object? sender, EventArgs e)
    {
        UpdateBorders();
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent == null)
        {
            DataGrid.ItemSelected -= DataGrid_ItemSelected;
            DataGrid.Columns.CollectionChanged -= OnColumnsChanged;
            DataGrid.RowsBackgroundColorPaletteChanged -= OnRowsBackgroundColorPaletteChanged;
            DataGrid.RowsTextColorPaletteChanged -= OnRowsTextColorPaletteChanged;
            DataGrid.BorderThicknessChanged -= OnBorderThicknessChanged;

            foreach (var column in DataGrid.Columns)
            {
                column.VisibilityChanged -= OnVisibilityChanged;
            }
        }
        else
        {
            DataGrid.ItemSelected += DataGrid_ItemSelected;
            DataGrid.Columns.CollectionChanged += OnColumnsChanged;
            DataGrid.RowsBackgroundColorPaletteChanged += OnRowsBackgroundColorPaletteChanged;
            DataGrid.RowsTextColorPaletteChanged += OnRowsTextColorPaletteChanged;
            DataGrid.BorderThicknessChanged += OnBorderThicknessChanged;

            foreach (var column in DataGrid.Columns)
            {
                column.VisibilityChanged += OnVisibilityChanged;
            }

            SetBinding(BackgroundColorProperty, new Binding(nameof(DataGrid.BorderColor), source: DataGrid));
        }
    }

    private void OnRowsTextColorPaletteChanged(object? sender, EventArgs e)
    {
        UpdateColors();
    }

    private void OnRowsBackgroundColorPaletteChanged(object? sender, EventArgs e)
    {
        UpdateColors();
    }

    private void OnColumnsChanged(object? sender, EventArgs e)
    {
        InitializeRow();
    }

    private void OnVisibilityChanged(object? sender, EventArgs e)
    {
        InitializeRow();
    }

    private void DataGrid_ItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_wasSelected || (e.CurrentSelection.Count > 0 && e.CurrentSelection.Any(s => s == BindingContext)))
        {
            UpdateSelectedState();
            UpdateColors();
        }
    }

    private void UpdateSelectedState()
    {
        _wasSelected = DataGrid.SelectedItem == BindingContext || DataGrid.SelectedItems.Contains(BindingContext);
    }

    #endregion Methods
}
