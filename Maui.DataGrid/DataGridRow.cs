namespace Maui.DataGrid;

using Utils;

internal sealed class DataGridRow : Grid
{
    #region Fields

    private Color _bgColor;
    private Color _textColor;
    private bool _hasSelected;

    #endregion

    #region properties

    public DataGrid DataGrid
    {
        get => (DataGrid)GetValue(DataGridProperty);
        set => SetValue(DataGridProperty, value);
    }

    #endregion

    #region Bindable Properties

    public static readonly BindableProperty DataGridProperty =
        BindableProperty.Create(nameof(DataGrid), typeof(DataGrid), typeof(DataGridRow), null,
            propertyChanged: (b, _, _) => ((DataGridRow)b).CreateView());

    #endregion

    #region Methods

    private void CreateView()
    {
        UpdateBackgroundColor();
        BackgroundColor = DataGrid.BorderColor;
        ColumnSpacing = DataGrid.BorderThickness.HorizontalThickness / 2;
        Padding = new Thickness(DataGrid.BorderThickness.HorizontalThickness / 2,
            DataGrid.BorderThickness.VerticalThickness / 2);

        foreach (var col in DataGrid.Columns)
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = col.Width });
            View cell;

            if (col.CellTemplate != null)
            {
                cell = new ContentView { Content = col.CellTemplate.CreateContent() as View };
                if (col.PropertyName != null)
                {
                    cell.SetBinding(BindingContextProperty,
                        new Binding(col.PropertyName, source: BindingContext));
                }
            }
            else
            {
                cell = new Label
                {
                    Padding = 0,
                    TextColor = _textColor,
                    BackgroundColor = _bgColor,
                    VerticalOptions = LayoutOptions.Fill,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalTextAlignment = col.VerticalContentAlignment.ToTextAlignment(),
                    HorizontalTextAlignment = col.HorizontalContentAlignment.ToTextAlignment(),
                    LineBreakMode = col.LineBreakMode
                };
                cell.SetBinding(Label.TextProperty,
                    new Binding(col.PropertyName, BindingMode.Default, stringFormat: col.StringFormat));
                cell.SetBinding(Label.FontSizeProperty,
                    new Binding(DataGrid.FontSizeProperty.PropertyName, BindingMode.Default, source: DataGrid));
                cell.SetBinding(Label.FontFamilyProperty,
                    new Binding(DataGrid.FontFamilyProperty.PropertyName, BindingMode.Default, source: DataGrid));
            }

            Children.Add(cell);
            SetColumn((BindableObject)cell, DataGrid.Columns.IndexOf(col));
        }
    }

    private void UpdateBackgroundColor()
    {
        _hasSelected = DataGrid?.SelectedItem == BindingContext;
        var actualIndex = DataGrid?.InternalItems?.IndexOf(BindingContext) ?? -1;
        if (actualIndex > -1)
        {
            _bgColor =
                DataGrid.SelectionEnabled && DataGrid.SelectedItem != null && DataGrid.SelectedItem == BindingContext
                    ? DataGrid.ActiveRowColor
                    : DataGrid.RowsBackgroundColorPalette.GetColor(actualIndex, BindingContext);
            _textColor = DataGrid.RowsTextColorPalette.GetColor(actualIndex, BindingContext);

            ChangeColor(_bgColor, _textColor);
        }
    }

    private void ChangeColor(Color bgColor, Color textColor)
    {
        foreach (var v in Children)
        {
            if (v is View view)
            {
                view.BackgroundColor = bgColor;

                if (view is Label label)
                {
                    label.TextColor = textColor;
                }
            }
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        UpdateBackgroundColor();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (DataGrid.SelectionEnabled)
        {
            if (Parent != null)
            {
                DataGrid.ItemSelected += DataGrid_ItemSelected;
            }
            else
            {
                DataGrid.ItemSelected -= DataGrid_ItemSelected;
            }
        }
    }

    private void DataGrid_ItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (DataGrid.SelectionEnabled && (e.CurrentSelection[^1] == BindingContext || _hasSelected))
        {
            UpdateBackgroundColor();
        }
    }

    #endregion
}