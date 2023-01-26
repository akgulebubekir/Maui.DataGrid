namespace Maui.DataGrid;

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

    public int Index
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    public object RowContext
    {
        get => GetValue(RowContextProperty);
        set => SetValue(RowContextProperty, value);
    }

    #endregion

    #region Bindable Properties

    public static readonly BindableProperty DataGridProperty =
        BindableProperty.Create(nameof(DataGrid), typeof(DataGrid), typeof(DataGridRow), null,
            propertyChanged: (b, _, _) => ((DataGridRow)b).CreateView());

    public static readonly BindableProperty IndexProperty =
        BindableProperty.Create(nameof(Index), typeof(int), typeof(DataGridRow), 0,
            propertyChanged: (b, _, _) => ((DataGridRow)b).UpdateBackgroundColor());

    public static readonly BindableProperty RowContextProperty =
        BindableProperty.Create(nameof(RowContext), typeof(object), typeof(DataGridRow),
            propertyChanged: (b, _, _) => ((DataGridRow)b).UpdateBackgroundColor());

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
                        new Binding(col.PropertyName, source: RowContext));
                }
            }
            else
            {
                var text = new Label
                {
                    TextColor = _textColor,
                    HorizontalOptions = col.HorizontalContentAlignment,
                    VerticalOptions = col.VerticalContentAlignment,
                    LineBreakMode = LineBreakMode.WordWrap
                };
                text.SetBinding(Label.TextProperty,
                    new Binding(col.PropertyName, BindingMode.Default, stringFormat: col.StringFormat));
                text.SetBinding(Label.FontSizeProperty,
                    new Binding(DataGrid.FontSizeProperty.PropertyName, BindingMode.Default, source: DataGrid));
                text.SetBinding(Label.FontFamilyProperty,
                    new Binding(DataGrid.FontFamilyProperty.PropertyName, BindingMode.Default, source: DataGrid));

                cell = new ContentView
                {
                    Padding = 0,
                    BackgroundColor = _bgColor,
                    Content = text
                };
            }

            Children.Add(cell);
            SetColumn((BindableObject)cell, DataGrid.Columns.IndexOf(col));
        }
    }

    private void UpdateBackgroundColor()
    {
        _hasSelected = DataGrid?.SelectedItem == RowContext;
        var actualIndex = DataGrid?.InternalItems?.IndexOf(BindingContext) ?? -1;
        if (actualIndex > -1)
        {
            _bgColor =
                DataGrid.SelectionEnabled && DataGrid.SelectedItem != null && DataGrid.SelectedItem == RowContext
                    ? DataGrid.ActiveRowColor
                    : DataGrid.RowsBackgroundColorPalette.GetColor(actualIndex, BindingContext);
            _textColor = DataGrid.RowsTextColorPalette.GetColor(actualIndex, BindingContext);

            ChangeColor(_bgColor);
        }
    }

    private void ChangeColor(Color color)
    {
        foreach (var v in Children)
        {
            var contentView = v as ContentView;
            contentView.BackgroundColor = color;
            if (contentView?.Content is Label label)
            {
                label.TextColor = _textColor;
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
        if (Parent != null)
        {
            DataGrid.ItemSelected += DataGrid_ItemSelected;
        }
        else
        {
            DataGrid.ItemSelected -= DataGrid_ItemSelected;
        }
    }

    private void DataGrid_ItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (DataGrid.SelectionEnabled && (e.CurrentSelection[^1] == RowContext || _hasSelected))
        {
            UpdateBackgroundColor();
        }
    }

    #endregion
}