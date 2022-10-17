using System.ComponentModel;

namespace Maui.DataGrid;

/// <summary>
/// Specifies each column of the DataGrid.
/// </summary>
public class DataGridColumn : BindableObject, IDefinition
{
    public DataGridColumn()
    {
        HeaderLabel = new Label();
        SortingIcon = new Image();
    }

    public event EventHandler SizeChanged;

    private void OnSizeChanged()
    {
        SizeChanged?.Invoke(this, EventArgs.Empty);
    }

    #region Bindable Properties

    public static readonly BindableProperty WidthProperty =
        BindableProperty.Create(nameof(Width), typeof(GridLength), typeof(DataGridColumn),
            GridLength.Star,
            propertyChanged: (b, o, n) =>
            {
                if (o != n)
                {
                    ((DataGridColumn)b).OnSizeChanged();
                }
            });

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DataGridColumn), string.Empty,
            propertyChanged: (b, _, n) => ((DataGridColumn)b).HeaderLabel.Text = (string)n);

    public static readonly BindableProperty FormattedTitleProperty =
        BindableProperty.Create(nameof(FormattedTitle), typeof(FormattedString), typeof(DataGridColumn),
            propertyChanged: (b, _, n) => ((DataGridColumn)b).HeaderLabel.FormattedText = (FormattedString)n);

    public static readonly BindableProperty PropertyNameProperty =
        BindableProperty.Create(nameof(PropertyName), typeof(string), typeof(DataGridColumn));

    public static readonly BindableProperty StringFormatProperty =
        BindableProperty.Create(nameof(StringFormat), typeof(string), typeof(DataGridColumn));

    public static readonly BindableProperty CellTemplateProperty =
        BindableProperty.Create(nameof(CellTemplate), typeof(DataTemplate), typeof(DataGridColumn));

    public static readonly BindableProperty HorizontalContentAlignmentProperty =
        BindableProperty.Create(nameof(HorizontalContentAlignment), typeof(LayoutOptions), typeof(DataGridColumn),
            LayoutOptions.Center);

    public static readonly BindableProperty VerticalContentAlignmentProperty =
        BindableProperty.Create(nameof(VerticalContentAlignment), typeof(LayoutOptions), typeof(DataGridColumn),
            LayoutOptions.Center);

    public static readonly BindableProperty SortingEnabledProperty =
        BindableProperty.Create(nameof(SortingEnabled), typeof(bool), typeof(DataGridColumn), true);

    public static readonly BindableProperty HeaderLabelStyleProperty =
        BindableProperty.Create(nameof(HeaderLabelStyle), typeof(Style), typeof(DataGridColumn),
            propertyChanged: (b, o, n) =>
            {
                if (((DataGridColumn)b).HeaderLabel != null && o != n)
                {
                    ((DataGridColumn)b).HeaderLabel.Style = n as Style;
                }
            });

    #endregion

    #region properties

    /// <summary>
    /// Width of the column. Like Grid, you can use <code>Absolute, star, Auto</code> as unit.
    /// </summary>
    [TypeConverter(typeof(GridLengthTypeConverter))]
    public GridLength Width
    {
        get => (GridLength)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    /// <summary>
    /// Column title
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Formatted title for column
    /// <example>
    /// <code>
    ///  &lt;DataGridColumn.FormattedTitle &gt;
    ///     &lt;FormattedString &gt;
    ///       &lt;Span Text = "Home" TextColor="Black" FontSize="13" FontAttributes="Bold" / &gt;
    ///       &lt;Span Text = " (win-loose)" TextColor="#333333" FontSize="11" / &gt;
    ///     &lt;/FormattedString &gt;
    ///  &lt;/DataGridColumn.FormattedTitle &gt;
    /// </code>
    /// </example>
    /// </summary>
    public FormattedString FormattedTitle
    {
        get => (string)GetValue(FormattedTitleProperty);
        set => SetValue(FormattedTitleProperty, value);
    }

    /// <summary>
    /// Property name to bind in the object
    /// </summary>
    public string PropertyName
    {
        get => (string)GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    /// <summary>
    /// String format for the cell
    /// </summary>
    public string StringFormat
    {
        get => (string)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    /// <summary>
    /// Cell template. Default value is  <c>Label</c> with binding <code>PropertyName</code>
    /// </summary>
    public DataTemplate CellTemplate
    {
        get => (DataTemplate)GetValue(CellTemplateProperty);
        set => SetValue(CellTemplateProperty, value);
    }

    internal Image SortingIcon { get; set; }
    internal Label HeaderLabel { get; set; }

    /// <summary>
    /// Horizontal alignment of the cell content 
    /// </summary>
    public LayoutOptions HorizontalContentAlignment
    {
        get => (LayoutOptions)GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Vertical alignment of the cell content 
    /// </summary>
    public LayoutOptions VerticalContentAlignment
    {
        get => (LayoutOptions)GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Defines if the column is sortable. Default is true
    /// </summary>
    public bool SortingEnabled
    {
        get => (bool)GetValue(SortingEnabledProperty);
        set => SetValue(SortingEnabledProperty, value);
    }

    /// <summary>
    /// Label Style of the header. <c>TargetType</c> must be Label. 
    /// </summary>
    public Style HeaderLabelStyle
    {
        get => (Style)GetValue(HeaderLabelStyleProperty);
        set => SetValue(HeaderLabelStyleProperty, value);
    }

    #endregion
}