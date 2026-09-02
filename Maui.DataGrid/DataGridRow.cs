namespace Maui.DataGrid;

using System.Diagnostics.CodeAnalysis;
using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls;

[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via XAML")]
internal sealed class DataGridRow : Grid
{
    #region Bindable Properties

    public static readonly BindableProperty DataGridProperty =
        BindablePropertyExtensions.Create<DataGridRow, DataGrid>(
            null,
            BindingMode.OneTime,
            propertyChanged: (b, o, _) =>
            {
                if (b is not DataGridRow dataGridRow)
                {
                    return;
                }

                if (o is DataGrid oldDataGrid)
                {
                    foreach (var column in oldDataGrid.Columns)
                    {
                        column.VisibilityChanged -= dataGridRow.OnVisibilityChanged;
                    }
                }

                foreach (var column in dataGridRow.DataGrid.Columns)
                {
                    column.VisibilityChanged -= dataGridRow.OnVisibilityChanged;
                    column.VisibilityChanged += dataGridRow.OnVisibilityChanged;
                }
            });

    public static readonly BindableProperty RowToEditProperty =
        BindablePropertyExtensions.Create<DataGridRow, object>(
            null,
            BindingMode.OneWay,
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

    /// <summary>
    /// Gets or sets the background color of the cells within this DataGridRow.
    /// </summary>
    public static readonly BindableProperty CellBackgroundColorProperty =
        BindablePropertyExtensions.Create<DataGridRow, Color>(
            defaultValue: Colors.White,
            propertyChanged: (b, _, n) =>
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
        BindablePropertyExtensions.Create<DataGridRow, Color>(
            defaultValue: Colors.White,
            propertyChanged: (b, _, n) =>
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

    #region Fields

    private bool _wasSelected;
    private TapGestureRecognizer? _tapGestureRecognizer;

    #endregion Fields

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

    /// <summary>
    /// Gets a value indicating whether this row is the one being edited. A row which the
    /// CollectionView has recycled out of view has no item, and an absent item is never the item
    /// being edited, even when nothing is being edited at all.
    /// </summary>
    private bool IsEditing => BindingContext != null && RowToEdit == BindingContext;

    #endregion Properties

    #region Methods

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        InitializeRow();
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        // Always unsubscribe first to prevent duplicate handlers
        DataGrid.ItemSelected -= DataGrid_ItemSelected;
        DataGrid.Columns.CollectionChanged -= OnColumnsChanged;
        DataGrid.RowsBackgroundColorPaletteChanged -= OnRowsBackgroundColorPaletteChanged;
        DataGrid.RowsTextColorPaletteChanged -= OnRowsTextColorPaletteChanged;
        DataGrid.RowTappedCommandModeChanged -= OnRowTappedCommandModeChanged;
        DataGrid.InternalItemsChanged -= OnInternalItemsChanged;

        foreach (var column in DataGrid.Columns)
        {
            column.VisibilityChanged -= OnVisibilityChanged;
        }

        if (Parent != null)
        {
            DataGrid.ItemSelected += DataGrid_ItemSelected;
            DataGrid.Columns.CollectionChanged += OnColumnsChanged;
            DataGrid.RowsBackgroundColorPaletteChanged += OnRowsBackgroundColorPaletteChanged;
            DataGrid.RowsTextColorPaletteChanged += OnRowsTextColorPaletteChanged;
            DataGrid.RowTappedCommandModeChanged += OnRowTappedCommandModeChanged;
            DataGrid.InternalItemsChanged += OnInternalItemsChanged;

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

        UpdateRowTapGesture();
    }

    private static Color InverseColor(Color color)
    {
        var brightness = (0.299 * color.Red) + (0.587 * color.Green) + (0.114 * color.Blue);
        return brightness < 0.5 ? Colors.White : Colors.Black;
    }

    /// <summary>
    /// Brings this row in line with the current columns and binding context, reusing the cells it
    /// already has. The CollectionView recycles rows by swapping their binding context, so cells are
    /// only created or replaced when a column, its visibility, or the editing state actually changed.
    /// Cell content follows the row's binding context through its bindings, so reused cells display
    /// the recycled row's item without being rebuilt.
    /// </summary>
    private void InitializeRow()
    {
        UpdateSelectedState();

        UpdateColors();

        var columns = DataGrid.Columns;

        if (columns == null || columns.Count == 0)
        {
            ColumnDefinitions.Clear();
            Children.Clear();
            return;
        }

        var isEditing = IsEditing;

        var columnCount = columns.Count;

        // Invisible columns still occupy a column definition, but they have no cell,
        // so the cell index trails the column index.
        var cellIndex = 0;

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

            if (Children.TryGetItem(cellIndex, out var existingChild))
            {
                if (existingChild is not DataGridCell existingCell)
                {
                    throw new InvalidDataException($"{nameof(DataGridRow)} should only contain {nameof(DataGridCell)}s");
                }

                if (CanReuseCell(existingCell, col, isEditing))
                {
                    // The cell is reusable as is, but a hidden or removed column may have shifted it.
                    SetColumn((BindableObject)existingCell, i);
                }
                else
                {
                    Children[cellIndex] = GenerateCellForColumn(col, i);
                }
            }
            else
            {
                var newCell = GenerateCellForColumn(col, i);
                Children.Add(newCell);
            }

            cellIndex++;
        }

        // Remove cells belonging to columns which are gone or no longer visible
        while (Children.Count > cellIndex)
        {
            Children.RemoveAt(Children.Count - 1);
        }

        // Remove extra columns, if any
        ColumnDefinitions.RemoveAfter(columnCount);
    }

    /// <summary>
    /// Determines whether an existing cell can be kept as is. A cell survives its row being recycled
    /// onto another item, unless it belongs to a different column, is in the wrong editing state, or a
    /// <see cref="DataTemplateSelector"/> now picks a different template for the row's item.
    /// </summary>
    private bool CanReuseCell(DataGridCell cell, DataGridColumn col, bool isEditing)
    {
        if (cell.Column != col || cell.IsEditing != isEditing)
        {
            return false;
        }

        var template = isEditing ? col.EditCellTemplate : col.CellTemplate;

        return template is not DataTemplateSelector selector
            || cell.ContentTemplate == selector.SelectTemplate(BindingContext, this);
    }

    /// <summary>
    /// Determines the template a cell's content is created from, resolving a
    /// <see cref="DataTemplateSelector"/> against this row's item. Calling <c>CreateContent()</c> on a
    /// selector silently yields an empty <see cref="Label"/> instead of the selected template.
    /// </summary>
    private DataTemplate? ResolveCellTemplate(DataGridColumn col, bool isEditing)
    {
        var template = isEditing ? col.EditCellTemplate : col.CellTemplate;

        return template is DataTemplateSelector selector
            ? selector.SelectTemplate(BindingContext, this)
            : template;
    }

    private DataGridCell GenerateCellForColumn(DataGridColumn col, int columnIndex)
    {
        var dataGridCell = CreateCell(col);

        dataGridCell.UpdateBindings(DataGrid);

        SetColumn((BindableObject)dataGridCell, columnIndex);

        return dataGridCell;
    }

    private DataGridCell CreateCell(DataGridColumn col)
    {
        var isEditing = IsEditing;

        var contentTemplate = ResolveCellTemplate(col, isEditing);

        var cellContent = isEditing
            ? CreateEditCell(col, contentTemplate)
            : CreateViewCell(col, contentTemplate);

        return new DataGridCell(cellContent, CellBackgroundColor, col, isEditing, contentTemplate);
    }

    private View CreateViewCell(DataGridColumn col, DataTemplate? cellTemplate)
    {
        View cell;

        if (cellTemplate != null)
        {
            cell = (View)cellTemplate.CreateContent();

            SetBinding(col, cell, BindingContextProperty);
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
                FontFamily = DataGrid.FontFamily,
                Padding = col.Padding,
            };

            SetBinding(col, cell, Label.TextProperty);
        }

        return cell;
    }

    private View CreateEditCell(DataGridColumn col, DataTemplate? editCellTemplate)
    {
        if (editCellTemplate == null)
        {
            return CreateDefaultEditCell(col);
        }

        var cell = (View)editCellTemplate.CreateContent();

        SetBinding(col, cell, BindingContextProperty);

        return cell;
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

    private Entry GenerateTextEditCell(DataGridColumn col)
    {
        var entry = new Entry
        {
            TextColor = CellTextColor,
            VerticalTextAlignment = col.VerticalTextAlignment,
            HorizontalTextAlignment = col.HorizontalTextAlignment,
            FontSize = DataGrid.FontSize,
            FontFamily = DataGrid.FontFamily,
        };

        SetBinding(col, entry, Entry.TextProperty);

        return entry;
    }

    private CheckBox GenerateBooleanEditCell(DataGridColumn col)
    {
        var checkBox = new CheckBox
        {
            Color = CellTextColor,
            BackgroundColor = CellBackgroundColor,
        };

        SetBinding(col, checkBox, CheckBox.IsCheckedProperty);

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
            Keyboard = Keyboard.Numeric,
        };

        entry.TextChanged += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.NewTextValue) && !numericParser(e.NewTextValue))
            {
                ((Entry)s!).Text = e.OldTextValue;
            }
        };

        SetBinding(col, entry, Entry.TextProperty);

        return entry;
    }

    private DatePicker GenerateDateTimeEditCell(DataGridColumn col)
    {
        var datePicker = new DatePicker
        {
            TextColor = CellTextColor,
        };

        SetBinding(col, datePicker, DatePicker.DateProperty);

        return datePicker;
    }

    /// <summary>
    /// Binds a cell view to its column's property. The binding is relative to the row rather than to
    /// the item, so that it follows the row when the CollectionView recycles it onto another item.
    /// Binding to the item directly would pin the cell to whichever item the row happened to hold when
    /// the cell was built, which is why cells used to be thrown away on every recycle.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Reflection is needed here.")]
    private void SetBinding(DataGridColumn col, View view, BindableProperty bindableProperty)
    {
        if (string.IsNullOrWhiteSpace(col.PropertyName))
        {
            return;
        }

        // A column bound to the item itself needs no property appended, since appending one would
        // produce the invalid path "BindingContext.". Such a binding is also one-way, because writing
        // back through it would replace the row's item rather than one of the item's properties.
        var binding = col.PropertyName == Binding.SelfPath
            ? new Binding(nameof(BindingContext), BindingMode.OneWay, stringFormat: col.StringFormat, source: this)
            : new Binding($"{nameof(BindingContext)}.{col.PropertyName}", BindingMode.TwoWay, stringFormat: col.StringFormat, source: this);

        view.SetBinding(bindableProperty, binding);
    }

    private void UpdateColors()
    {
        var rowIndex = DataGrid.GetItemIndex(BindingContext);

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

    private void OnRowsTextColorPaletteChanged(object? sender, EventArgs e)
    {
        UpdateColors();
    }

    private void OnInternalItemsChanged(object? sender, EventArgs e)
    {
        // This row's index may have shifted, even though its item did not change.
        UpdateColors();
    }

    private void OnRowsBackgroundColorPaletteChanged(object? sender, EventArgs e)
    {
        UpdateColors();
    }

    /// <summary>
    /// Adds or removes the row's tap gesture to match <see cref="DataGrid.RowTappedCommandMode"/>.
    /// The gesture is only attached when it is needed, so that rows behave exactly as before for
    /// consumers who have not opted into <see cref="RowTappedCommandMode.Tap"/>.
    /// </summary>
    private void UpdateRowTapGesture()
    {
        if (Parent != null && DataGrid.RowTappedCommandMode == RowTappedCommandMode.Tap)
        {
            _tapGestureRecognizer ??= new TapGestureRecognizer { Command = new Command(ExecuteRowTappedCommand) };

            if (!GestureRecognizers.Contains(_tapGestureRecognizer))
            {
                GestureRecognizers.Add(_tapGestureRecognizer);
            }
        }
        else if (_tapGestureRecognizer != null)
        {
            _ = GestureRecognizers.Remove(_tapGestureRecognizer);
        }
    }

    /// <summary>
    /// Executes <see cref="DataGrid.RowTappedCommand"/> with this row's item. Unlike the
    /// selection-changed path, this fires for every tap, including a tap on the already selected
    /// row and any tap while selection is disabled.
    /// </summary>
    private void ExecuteRowTappedCommand()
    {
        var rowTappedCommand = DataGrid.RowTappedCommand;

        if (rowTappedCommand?.CanExecute(BindingContext) == true)
        {
            rowTappedCommand.Execute(BindingContext);
        }
    }

    private void OnRowTappedCommandModeChanged(object? sender, EventArgs e)
    {
        UpdateRowTapGesture();
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
