namespace Maui.DataGrid;

using Microsoft.Maui.Controls.Shapes;
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

        ColumnSpacing = DataGrid.BorderThickness;
        Margin = new Thickness(0, 0, 0, DataGrid.BorderThickness);

        for (int i = 0; i < DataGrid.Columns.Count; i++)
        {
            DataGridColumn col = DataGrid.Columns[i];
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

            var border = new ContentView
            {
                Content = cell,
                HeightRequest = DataGrid.RowHeight
            };

            Children.Add(border);
            SetColumn((BindableObject)border, i);
        }
    }

    private void UpdateBackgroundColor()
    {
        _hasSelected = DataGrid?.SelectedItem == BindingContext;
        var actualIndex = DataGrid?.InternalItems?.IndexOf(BindingContext) ?? -1;
        if (actualIndex > -1)
        {
            var backgroundColor =
                DataGrid.SelectionEnabled && DataGrid.SelectedItem != null && DataGrid.SelectedItem == BindingContext
                    ? DataGrid.ActiveRowColor
                    : DataGrid.RowsBackgroundColorPalette.GetColor(actualIndex, BindingContext);
            TextColor = DataGrid.RowsTextColorPalette.GetColor(actualIndex, BindingContext);

            ChangeColor(backgroundColor, TextColor);
        }
    }

    private void ChangeColor(Color backgroundColor, Color textColor)
    {
        foreach (var child in Children)
        {
            if (child is View view)
            {
                view.BackgroundColor = backgroundColor;

                if (view is ContentView contentView && contentView.Content is Label label)
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