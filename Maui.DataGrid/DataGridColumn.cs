namespace Maui.DataGrid;

using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;

/// <summary>
/// Specifies each column of the DataGrid.
/// </summary>
public sealed class DataGridColumn : BindableObject, IDefinition
{
    private bool? isSortable;
    private ColumnDefinition? columnDefinition;
    private readonly ColumnDefinition invisibleColumnDefinition = new(0);

    public DataGridColumn()
    {
        this.HeaderLabel = new();
        this.SortingIcon = new();
        this.SortingIconContainer = new ContentView
        {
            IsVisible = false,
            Content = this.SortingIcon,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
    }

    private readonly WeakEventManager sizeChangedEventManager = new();

    public event EventHandler SizeChanged
    {
        add => this.sizeChangedEventManager.AddEventHandler(value);
        remove => this.sizeChangedEventManager.RemoveEventHandler(value);
    }

    private void OnSizeChanged() => this.sizeChangedEventManager.HandleEvent(this, EventArgs.Empty, string.Empty);

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

    public static readonly BindableProperty IsVisibleProperty =
        BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(DataGridColumn), true,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGridColumn column)
                {
                    var dataGrid = (DataGrid)column.HeaderLabel.Parent.Parent.Parent.Parent;
                    dataGrid.Reload();
                    column.OnSizeChanged();
                }
            });

    public static readonly BindableProperty StringFormatProperty =
        BindableProperty.Create(nameof(StringFormat), typeof(string), typeof(DataGridColumn));

    public static readonly BindableProperty CellTemplateProperty =
        BindableProperty.Create(nameof(CellTemplate), typeof(DataTemplate), typeof(DataGridColumn));

    public static readonly BindableProperty LineBreakModeProperty =
        BindableProperty.Create(nameof(LineBreakMode), typeof(LineBreakMode), typeof(DataGridColumn),
            LineBreakMode.WordWrap);

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

    public ColumnDefinition? ColumnDefinition
    {
        get
        {
            if (!this.IsVisible)
            {
                return this.invisibleColumnDefinition;
            }

            return this.columnDefinition;
        }

        internal set => this.columnDefinition = value;
    }

    /// <summary>
    /// Gets or sets width of the column. Like Grid, you can use <c>Absolute, star, Auto</c> as unit.
    /// </summary>
    [TypeConverter(typeof(GridLengthTypeConverter))]
    public GridLength Width
    {
        get => (GridLength)this.GetValue(WidthProperty);
        set
        {
            this.SetValue(WidthProperty, value);
            this.ColumnDefinition = new(value);
        }
    }

    /// <summary>
    /// Gets or sets column title.
    /// </summary>
    public string Title
    {
        get => (string)this.GetValue(TitleProperty);
        set => this.SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets formatted title for column.
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
        get => (string)this.GetValue(FormattedTitleProperty);
        set => this.SetValue(FormattedTitleProperty, value);
    }

    /// <summary>
    /// Gets or sets property name to bind in the object.
    /// </summary>
    public string PropertyName
    {
        get => (string)this.GetValue(PropertyNameProperty);
        set => this.SetValue(PropertyNameProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether is this column visible?.
    /// </summary>
    public bool IsVisible
    {
        get => (bool)this.GetValue(IsVisibleProperty);
        set => this.SetValue(IsVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets string format for the cell.
    /// </summary>
    public string StringFormat
    {
        get => (string)this.GetValue(StringFormatProperty);
        set => this.SetValue(StringFormatProperty, value);
    }

    /// <summary>
    /// Gets or sets cell template. Default value is <c>Label</c> with binding <c>PropertyName</c>.
    /// </summary>
    public DataTemplate CellTemplate
    {
        get => (DataTemplate)this.GetValue(CellTemplateProperty);
        set => this.SetValue(CellTemplateProperty, value);
    }

    internal Polygon SortingIcon { get; }
    internal Label HeaderLabel { get; }
    internal View SortingIconContainer { get; }

    /// <summary>
    /// Gets or sets lineBreakModeProperty for the text. WordWrap by default.
    /// </summary>
    public LineBreakMode LineBreakMode
    {
        get => (LineBreakMode)this.GetValue(LineBreakModeProperty);
        set => this.SetValue(LineBreakModeProperty, value);
    }

    /// <summary>
    /// Gets or sets horizontal alignment of the cell content.
    /// </summary>
    public LayoutOptions HorizontalContentAlignment
    {
        get => (LayoutOptions)this.GetValue(HorizontalContentAlignmentProperty);
        set => this.SetValue(HorizontalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets vertical alignment of the cell content.
    /// </summary>
    public LayoutOptions VerticalContentAlignment
    {
        get => (LayoutOptions)this.GetValue(VerticalContentAlignmentProperty);
        set => this.SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether defines if the column is sortable. Default is true
    /// Sortable columns must implement <see cref="IComparable"/>.
    /// </summary>
    public bool SortingEnabled
    {
        get => (bool)this.GetValue(SortingEnabledProperty);
        set => this.SetValue(SortingEnabledProperty, value);
    }

    /// <summary>
    /// Determines via reflection if the column's data type is sortable.
    /// If you want to disable sorting for specific column please use <c>SortingEnabled</c> property.
    /// </summary>
    /// <param name="dataGrid"></param>
    public bool IsSortable(DataGrid dataGrid)
    {
        if (this.isSortable is not null)
        {
            return this.isSortable.Value;
        }

        try
        {
            var listItemType = dataGrid.ItemsSource.GetType().GetGenericArguments().Single();
            var columnDataType = listItemType.GetProperty(this.PropertyName)?.PropertyType;

            if (columnDataType is not null)
            {
                this.isSortable = typeof(IComparable).IsAssignableFrom(columnDataType);
            }
        }
        catch
        {
            this.isSortable = false;
        }

        return this.isSortable ?? false;
    }

    /// <summary>
    /// Gets or sets label Style of the header. <c>TargetType</c> must be Label.
    /// </summary>
    public Style HeaderLabelStyle
    {
        get => (Style)this.GetValue(HeaderLabelStyleProperty);
        set => this.SetValue(HeaderLabelStyleProperty, value);
    }
}
