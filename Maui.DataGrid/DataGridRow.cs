namespace Maui.DataGrid;

using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls;

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

    #endregion Properties

    #region Bindable Properties

    public static readonly BindableProperty DataGridProperty =
        BindablePropertyExtensions.Create<DataGrid>(null, BindingMode.OneTime,
            propertyChanged: (b, o, n) =>
            {
                var self = (DataGridRow)b;

                if (o is DataGrid oldDataGrid)
                {
                    oldDataGrid.ItemSelected -= self.DataGrid_ItemSelected;
                }

                if (n is DataGrid newDataGrid && newDataGrid.SelectionEnabled)
                {
                    newDataGrid.ItemSelected += self.DataGrid_ItemSelected;
                }
            });

    #endregion Bindable Properties

    #region Methods

    private void CreateView()
    {
        ColumnDefinitions.Clear();
        Children.Clear();

        SetStyling();

        for (var i = 0; i < DataGrid.Columns.Count; i++)
        {
            var col = DataGrid.Columns[i];

            ColumnDefinitions.Add(col.ColumnDefinition);

            if (!col.IsVisible)
            {
                continue;
            }

            var cell = CreateCell(col);

            SetColumn((BindableObject)cell, i);
            Children.Add(cell);
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
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalTextAlignment = col.VerticalContentAlignment.ToTextAlignment(),
                HorizontalTextAlignment = col.HorizontalContentAlignment.ToTextAlignment(),
                LineBreakMode = col.LineBreakMode
            };

            if (!string.IsNullOrWhiteSpace(col.PropertyName))
            {
                cell.SetBinding(Label.TextProperty,
                    new Binding(col.PropertyName, BindingMode.Default, stringFormat: col.StringFormat, source: BindingContext));
            }
            cell.SetBinding(Label.FontSizeProperty,
                new Binding(DataGrid.FontSizeProperty.PropertyName, BindingMode.Default, source: DataGrid));
            cell.SetBinding(Label.FontFamilyProperty,
                new Binding(DataGrid.FontFamilyProperty.PropertyName, BindingMode.Default, source: DataGrid));
        }

        return cell;
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

        if (BindingContext != DataGrid.BindingContext)
        {
            CreateView();
        }
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent == null)
        {
            DataGrid.ItemSelected -= DataGrid_ItemSelected;
        }
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
