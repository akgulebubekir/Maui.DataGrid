namespace Maui.DataGrid;

using Microsoft.Maui.Controls;
using Utils;

internal sealed class DataGridRow : Grid
{
    private Color? bgColor;
    private Color? textColor;
    private bool hasSelected;

    public DataGrid DataGrid
    {
        get => (DataGrid)this.GetValue(DataGridProperty);
        set => this.SetValue(DataGridProperty, value);
    }

    public static readonly BindableProperty DataGridProperty =
        BindableProperty.Create(nameof(DataGrid), typeof(DataGrid), typeof(DataGridRow), null,
            propertyChanged: (b, _, _) => ((DataGridRow)b).CreateView());

    private void CreateView()
    {
        this.ColumnDefinitions.Clear();
        this.Children.Clear();

        this.BackgroundColor = this.DataGrid.BorderColor;

        var borderThickness = this.DataGrid.BorderThickness;

        this.Padding = new(borderThickness.Left, borderThickness.Top, borderThickness.Right, 0);
        this.ColumnSpacing = borderThickness.HorizontalThickness;
        this.Margin = new Thickness(0, 0, 0, borderThickness.Bottom); // Row Spacing

        for (var i = 0; i < this.DataGrid.Columns.Count; i++)
        {
            var col = this.DataGrid.Columns[i];

            this.ColumnDefinitions.Add(col.ColumnDefinition);

            View cell;

            if (col.CellTemplate != null)
            {
                cell = new ContentView { Content = col.CellTemplate.CreateContent() as View };
                if (col.PropertyName != null)
                {
                    cell.SetBinding(BindingContextProperty,
                        new Binding(col.PropertyName, source: this.BindingContext));
                }
            }
            else
            {
                cell = new Label
                {
                    TextColor = textColor,
                    BackgroundColor = bgColor,
                    VerticalOptions = LayoutOptions.Fill,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalTextAlignment = col.VerticalContentAlignment.ToTextAlignment(),
                    HorizontalTextAlignment = col.HorizontalContentAlignment.ToTextAlignment(),
                    LineBreakMode = col.LineBreakMode
                };
                cell.SetBinding(Label.TextProperty,
                    new Binding(col.PropertyName, BindingMode.Default, stringFormat: col.StringFormat));
                cell.SetBinding(Label.FontSizeProperty,
                    new Binding(DataGrid.FontSizeProperty.PropertyName, BindingMode.Default, source: this.DataGrid));
                cell.SetBinding(Label.FontFamilyProperty,
                    new Binding(DataGrid.FontFamilyProperty.PropertyName, BindingMode.Default, source: this.DataGrid));
            }

            cell.SetBinding(IsVisibleProperty,
                new Binding(nameof(col.IsVisible), BindingMode.OneWay, source: col));

            SetColumn((BindableObject)cell, i);
            this.Children.Add(cell);
        }

        this.UpdateBackgroundColor();
    }

    private void UpdateBackgroundColor()
    {
        this.hasSelected = this.DataGrid?.SelectedItem == this.BindingContext;
        var actualIndex = this.DataGrid?.InternalItems?.IndexOf(this.BindingContext) ?? -1;
        if (actualIndex > -1)
        {
            this.bgColor =
                this.DataGrid?.SelectionEnabled == true && this.DataGrid.SelectedItem != null && this.DataGrid.SelectedItem == this.BindingContext
                    ? this.DataGrid.ActiveRowColor
                    : this.DataGrid?.RowsBackgroundColorPalette.GetColor(actualIndex, this.BindingContext);
            this.textColor = this.DataGrid?.RowsTextColorPalette.GetColor(actualIndex, this.BindingContext);

            this.ChangeColor(this.bgColor, this.textColor);
        }
    }

    private void ChangeColor(Color? bgColor, Color? textColor)
    {
        foreach (var v in this.Children)
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
        this.CreateView();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (this.DataGrid.SelectionEnabled)
        {
            if (this.Parent != null)
            {
                this.DataGrid.ItemSelected += this.DataGrid_ItemSelected;
            }
            else
            {
                this.DataGrid.ItemSelected -= this.DataGrid_ItemSelected;
            }
        }
    }

    private void DataGrid_ItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (this.DataGrid.SelectionEnabled && (e.CurrentSelection[^1] == this.BindingContext || this.hasSelected))
        {
            this.UpdateBackgroundColor();
        }
    }
}
