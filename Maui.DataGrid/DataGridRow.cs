namespace Maui.DataGrid;

using System.Globalization;
using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

internal sealed class DataGridRow : Grid
{
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
                        column.SizeChanged -= self.OnSizeChanged;
                    }
                }

                if (n is DataGrid newDataGrid && newDataGrid.SelectionEnabled)
                {
                    newDataGrid.ItemSelected += self.DataGrid_ItemSelected;
                    newDataGrid.Columns.CollectionChanged += self.OnColumnsChanged;

                    foreach (var column in newDataGrid.Columns)
                    {
                        column.SizeChanged += self.OnSizeChanged;
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

        for (var i = 0; i < DataGrid.Columns.Count; i++)
        {
            var col = DataGrid.Columns[i];

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
                    new Binding(col.PropertyName, BindingMode.Default, stringFormat: col.StringFormat, source: BindingContext));
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

        switch (Type.GetTypeCode(col.DataType))
        {
            case TypeCode.String:
                return GenerateTextEditCell(col);
            case TypeCode.Boolean:
                return GenerateBooleanEditCell(col);
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.Single:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return GenerateNumericEditCell(col);
            case TypeCode.DateTime:
                return GenerateDateTimeEditCell(col);
        }

        return new TemplatedView { BackgroundColor = _bgColor };
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

    private Grid GenerateNumericEditCell(DataGridColumn col)
    {
        var stackLayout = new FlexLayout
        {
            Wrap = FlexWrap.Wrap,
            Direction = FlexDirection.Row,
            AlignContent = FlexAlignContent.Center,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustify.Center,
        };

        var label = new Label
        {
            Margin = new(0, 0, 3, 0),
            TextColor = _textColor,
            VerticalTextAlignment = TextAlignment.Center,
        };

        var stepper = new Stepper
        {
            BackgroundColor = DeviceInfo.Platform == DevicePlatform.WinUI ? _textColor : null,
        };

        stepper.ValueChanged += (b, e) =>
        {
            if (b is Stepper s)
            {
                label.Text = s.Value.ToString(CultureInfo.InvariantCulture);
            }
        };

        stackLayout.Add(label);
        stackLayout.Add(stepper);

        if (!string.IsNullOrWhiteSpace(col.PropertyName))
        {
            label.SetBinding(Label.TextProperty,
                new Binding(col.PropertyName, BindingMode.TwoWay, source: BindingContext));
            stepper.SetBinding(Stepper.ValueProperty,
                new Binding(col.PropertyName, BindingMode.TwoWay, source: BindingContext));
        }

        return WrapViewInGrid(stackLayout);
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
        }
    }

    private void OnColumnsChanged(object? sender, EventArgs e)
    {
        CreateView();
    }

    private void OnSizeChanged(object? sender, EventArgs e)
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
