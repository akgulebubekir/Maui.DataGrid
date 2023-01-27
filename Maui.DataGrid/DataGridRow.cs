namespace Maui.DataGrid;

using Utils;

internal sealed class DataGridRow : Grid
{
    #region Fields

    private bool _hasSelected;

    #endregion

    #region properties

    public DataGrid DataGrid
    {
        get => (DataGrid)GetValue(DataGridProperty);
        set => SetValue(DataGridProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    #endregion

    #region Bindable Properties

    public static readonly BindableProperty DataGridProperty =
        BindableProperty.Create(nameof(DataGrid), typeof(DataGrid), typeof(DataGridRow), null,
            propertyChanged: (b, _, _) => ((DataGridRow)b).CreateView());

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(DataGridRow), null);

    #endregion

    #region Methods

    private void CreateView()
    {
        UpdateBackgroundColor();
        ColumnSpacing = DataGrid.BorderThickness / 2;
        Padding = new Thickness(DataGrid.BorderThickness / 2);

        foreach (var col in DataGrid.Columns)
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = col.Width });

            if (col.CellTemplate?.CreateContent() is View cell)
            {
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
                    TextColor = TextColor,
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

            var border = new Border
            {
                BackgroundColor = Colors.Transparent,
                Content = cell,
                Stroke = new SolidColorBrush(DataGrid.BorderColor),
                StrokeThickness = DataGrid.BorderThickness,
                HeightRequest = DataGrid.RowHeight
            };

            Children.Add(border);
            SetColumn((BindableObject)border, DataGrid.Columns.IndexOf(col));
        }
    }

    private void UpdateBackgroundColor()
    {
        _hasSelected = DataGrid?.SelectedItem == BindingContext;
        var actualIndex = DataGrid?.InternalItems?.IndexOf(BindingContext) ?? -1;
        if (actualIndex > -1)
        {
            BackgroundColor =
                DataGrid.SelectionEnabled && DataGrid.SelectedItem != null && DataGrid.SelectedItem == BindingContext
                    ? DataGrid.ActiveRowColor
                    : DataGrid.RowsBackgroundColorPalette.GetColor(actualIndex, BindingContext);
            TextColor = DataGrid.RowsTextColorPalette.GetColor(actualIndex, BindingContext);

            ChangeColor(BackgroundColor, TextColor);
        }
    }

    private void ChangeColor(Color backgroundColor, Color textColor)
    {
        foreach (var child in Children)
        {
            if (child is Border border)
            {
                if (border.Content is Label label)
                {
                    label.BackgroundColor = backgroundColor;
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