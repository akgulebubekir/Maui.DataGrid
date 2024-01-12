namespace Maui.DataGrid;

using Extensions;
using Microsoft.Maui.Controls;

internal sealed class DataGridRow : Grid
{
    private delegate bool ParserDelegate(string value);

    #region Fields

    private Color? _bgColor;
    private Color? _textColor;
    private bool _hasSelected;

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

    #endregion Properties

    #region Bindable Properties

    public static readonly BindableProperty DataGridProperty =
        BindablePropertyExtensions.Create<DataGridRow, DataGrid>(null, BindingMode.OneTime,
            propertyChanged: (b, o, n) =>
            {
                var self = (DataGridRow)b;

                if (o is DataGrid oldDataGrid)
                {
                    oldDataGrid.ItemSelected -= self.DataGrid_ItemSelected;
                    oldDataGrid.Columns.CollectionChanged -= self.OnColumnsChanged;

                    foreach (var column in oldDataGrid.Columns)
                    {
                        column.VisibilityChanged -= self.OnVisibilityChanged;
                    }
                }

                if (n is DataGrid newDataGrid && newDataGrid.SelectionEnabled)
                {
                    newDataGrid.ItemSelected += self.DataGrid_ItemSelected;
                    newDataGrid.Columns.CollectionChanged += self.OnColumnsChanged;

                    foreach (var column in newDataGrid.Columns)
                    {
                        column.VisibilityChanged += self.OnVisibilityChanged;
                    }
                }
            });

    public static readonly BindableProperty RowToEditProperty =
        BindablePropertyExtensions.Create<DataGridRow, object>(null, BindingMode.OneWay,
            propertyChanged: (b, o, n) =>
            {
                if (o == n || b is not DataGridRow row)
                {
                    return;
                }

                if (o == row.BindingContext || n == row.BindingContext)
                {
                    row.CreateView();
                }
            });

    #endregion Bindable Properties

    #region Methods

    private void CreateView()
    {
        Children.Clear();

        SetStyling();

        if (DataGrid.Columns == null || DataGrid.Columns.Count == 0)
        {
            ColumnDefinitions.Clear();
            return;
        }

        for (var i = 0; i < DataGrid.Columns.Count; i++)
        {
            var col = DataGrid.Columns[i];

            // Add or update columns as needed
            if (i > ColumnDefinitions.Count - 1)
            {
                ColumnDefinitions.Add(col.ColumnDefinition);
            }
            else if (ColumnDefinitions[i] != col.ColumnDefinition)
            {
                ColumnDefinitions[i] = col.ColumnDefinition;
            }

            if (!col.IsVisible)
            {
                continue;
            }

            var cell = CreateCell(col);

            SetColumn((BindableObject)cell, i);
            Children.Add(cell);
        }

        // Remove extra columns
        for (var i = ColumnDefinitions.Count - 1; i > DataGrid.Columns.Count - 1; i--)
        {
            ColumnDefinitions.RemoveAt(i);
        }
    }

    private void SetStyling()
    {
        UpdateColors();

        // We are using the spacing between rows to generate visible borders, and thus the background color is the border color.
        BackgroundColor = DataGrid.BorderColor;

        var borderThickness = DataGrid.BorderThickness;

        Padding = new(borderThickness.Left, borderThickness.Top, borderThickness.Right, 0);
        ColumnSpacing = borderThickness.HorizontalThickness;
        Margin = new Thickness(0, 0, 0, borderThickness.Bottom); // Row Spacing
    }

    private View CreateCell(DataGridColumn col)
    {
        if (RowToEdit == BindingContext)
        {
            return CreateEditCell(col);
        }

        return CreateViewCell(col);
    }

    private View CreateViewCell(DataGridColumn col)
    {
        View cell;

        if (col.CellTemplate != null)
        {
            cell = new ContentView
            {
                BackgroundColor = _bgColor,
                Content = col.CellTemplate.CreateContent() as View
            };

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
                TextColor = _textColor,
                BackgroundColor = _bgColor,
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

        if (cell != null)
        {
            return cell;
        }

        return Type.GetTypeCode(col.DataType) switch
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
            _ => new TemplatedView { BackgroundColor = _bgColor },
        };
    }

    private ContentView? GenerateTemplatedEditCell(DataGridColumn col)
    {
        if (col.EditCellTemplate == null)
        {
            return null;
        }

        var cell = new ContentView
        {
            BackgroundColor = _bgColor,
            Content = col.EditCellTemplate.CreateContent() as View
        };

        if (!string.IsNullOrWhiteSpace(col.PropertyName))
        {
            cell.SetBinding(BindingContextProperty,
                new Binding(col.PropertyName, source: BindingContext));
        }

        return cell;
    }

    private Grid GenerateTextEditCell(DataGridColumn col)
    {
        var entry = new Entry
        {
            TextColor = _textColor,
            BackgroundColor = _bgColor,
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

        return WrapViewInGrid(entry);
    }

    private Grid GenerateBooleanEditCell(DataGridColumn col)
    {
        var checkBox = new CheckBox
        {
            Color = _textColor,
            BackgroundColor = _bgColor,
        };

        if (!string.IsNullOrWhiteSpace(col.PropertyName))
        {
            checkBox.SetBinding(CheckBox.IsCheckedProperty,
                new Binding(col.PropertyName, BindingMode.TwoWay, source: BindingContext));
        }

        return WrapViewInGrid(checkBox);
    }

    private Grid GenerateNumericEditCell(DataGridColumn col, ParserDelegate parserDelegate)
    {
        var entry = new Entry
        {
            TextColor = _textColor,
            BackgroundColor = _bgColor,
            VerticalTextAlignment = col.VerticalTextAlignment,
            HorizontalTextAlignment = col.HorizontalTextAlignment,
            FontSize = DataGrid.FontSize,
            FontFamily = DataGrid.FontFamily,
            Keyboard = Keyboard.Numeric
        };

        entry.TextChanged += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.NewTextValue) && !parserDelegate(e.NewTextValue))
            {
                ((Entry)s!).Text = e.OldTextValue;
            }
        };

        if (!string.IsNullOrWhiteSpace(col.PropertyName))
        {
            entry.SetBinding(Entry.TextProperty,
                new Binding(col.PropertyName, BindingMode.TwoWay, source: BindingContext));
        }

        return WrapViewInGrid(entry);
    }

    private Grid GenerateDateTimeEditCell(DataGridColumn col)
    {
        var datePicker = new DatePicker
        {
            TextColor = _textColor,
        };

        if (!string.IsNullOrWhiteSpace(col.PropertyName))
        {
            datePicker.SetBinding(DatePicker.DateProperty,
                new Binding(col.PropertyName, BindingMode.TwoWay, source: BindingContext));
        }

        return WrapViewInGrid(datePicker);
    }

    private Grid WrapViewInGrid(View view)
    {
        var grid = new Grid
        {
            BackgroundColor = _bgColor,
        };

        grid.Add(view);

        return grid;
    }

    private void UpdateColors()
    {
        _hasSelected = DataGrid.SelectedItem == BindingContext;
        var rowIndex = DataGrid.InternalItems?.IndexOf(BindingContext) ?? -1;

        if (rowIndex < 0)
        {
            return;
        }

        _bgColor = DataGrid.SelectionEnabled && _hasSelected
                ? DataGrid.ActiveRowColor
                : DataGrid.RowsBackgroundColorPalette.GetColor(rowIndex, BindingContext);
        _textColor = DataGrid.RowsTextColorPalette.GetColor(rowIndex, BindingContext);

        foreach (var v in Children)
        {
            if (v is View view)
            {
                view.BackgroundColor = _bgColor;

                if (view is Label label)
                {
                    label.TextColor = _textColor;
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        CreateView();
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent == null)
        {
            DataGrid.ItemSelected -= DataGrid_ItemSelected;
            DataGrid.Columns.CollectionChanged -= OnColumnsChanged;

            foreach (var column in DataGrid.Columns)
            {
                column.VisibilityChanged -= OnVisibilityChanged;
            }
        }
    }

    private void OnColumnsChanged(object? sender, EventArgs e)
    {
        CreateView();
    }

    private void OnVisibilityChanged(object? sender, EventArgs e)
    {
        CreateView();
    }

    private void DataGrid_ItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (!DataGrid.SelectionEnabled)
        {
            return;
        }

        if (_hasSelected || (e.CurrentSelection.Count > 0 && e.CurrentSelection[^1] == BindingContext))
        {
            UpdateColors();
        }
    }

    #endregion Methods
}
