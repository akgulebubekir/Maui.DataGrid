namespace Maui.DataGrid;

using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;

/// <summary>
/// Specifies each column of the DataGrid.
/// </summary>
public sealed class DataGridColumn : BindableObject, IDefinition
{
    #region Fields

    private bool? _isSortable;
    private ColumnDefinition? _columnDefinition;
    private readonly ColumnDefinition _invisibleColumnDefinition = new(0);
    private readonly WeakEventManager _sizeChangedEventManager = new();

    #endregion Fields

    public DataGridColumn()
    {
        HeaderLabel = new();
        SortingIcon = new();
        SortingIconContainer = new ContentView
        {
            IsVisible = false,
            Content = SortingIcon,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
    }

    #region Events

    public event EventHandler SizeChanged
    {
        add => _sizeChangedEventManager.AddEventHandler(value);
        remove => _sizeChangedEventManager.RemoveEventHandler(value);
    }

    #endregion Events

    #region Bindable Properties

    public static readonly BindableProperty WidthProperty =
        BindableProperty.Create(nameof(Width), typeof(GridLength), typeof(DataGridColumn),
            GridLength.Star,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && n is GridLength width && b is DataGridColumn self)
                {
                    self.ColumnDefinition = new(width);
                    self.OnSizeChanged();
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
                    try
                    {
                        column.DataGrid ??= (DataGrid?)column.HeaderLabel.Parent?.Parent?.Parent?.Parent;
                        column.DataGrid?.Reload();
                    }
                    catch { }
                    finally
                    {
                        column.OnSizeChanged();
                    }
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
                if (o != n && b is DataGridColumn self && self.HeaderLabel != null)
                {
                    self.HeaderLabel.Style = n as Style;
                }
            });

    #endregion Bindable Properties

    #region Properties

    internal DataGrid? DataGrid { get; set; }

    internal ColumnDefinition? ColumnDefinition
    {
        get
        {
            if (!IsVisible)
            {
                return _invisibleColumnDefinition;
            }

            return _columnDefinition;
        }
        set => _columnDefinition = value;
    }

    internal View? HeaderView { get; set; }

    /// <summary>
    /// Width of the column. Like Grid, you can use <c>Absolute, star, Auto</c> as unit.
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
    ///       &lt;Span Text = " (won-lost)" TextColor="#333333" FontSize="11" / &gt;
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
    /// Is this column visible?
    /// </summary>
    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
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
    /// Cell template. Default value is <c>Label</c> with binding <c>PropertyName</c>
    /// </summary>
    public DataTemplate CellTemplate
    {
        get => (DataTemplate)GetValue(CellTemplateProperty);
        set => SetValue(CellTemplateProperty, value);
    }

    /// <summary>
    /// LineBreakModeProperty for the text. WordWrap by default.
    /// </summary>
    public LineBreakMode LineBreakMode
    {
        get => (LineBreakMode)GetValue(LineBreakModeProperty);
        set => SetValue(LineBreakModeProperty, value);
    }

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
    /// Sortable columns must implement <see cref="IComparable"/>
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

    internal Polygon SortingIcon { get; }
    internal Label HeaderLabel { get; }
    internal View SortingIconContainer { get; }
    internal SortingOrder SortingOrder { get; set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Determines via reflection if the column's data type is sortable.
    /// If you want to disable sorting for specific column please use <c>SortingEnabled</c> property
    /// </summary>
    /// <param name="dataGrid"></param>
    public bool IsSortable(DataGrid dataGrid)
    {
        if (_isSortable is not null)
        {
            return _isSortable.Value;
        }

        try
        {
            var listItemType = dataGrid.ItemsSource.GetType().GetGenericArguments().Single();
            var columnDataType = listItemType.GetProperty(PropertyName)?.PropertyType;

            if (columnDataType is not null)
            {
                _isSortable = typeof(IComparable).IsAssignableFrom(columnDataType);
            }
        }
        catch
        {
            _isSortable = false;
        }

        return _isSortable ?? false;
    }

    private void OnSizeChanged() => _sizeChangedEventManager.HandleEvent(this, EventArgs.Empty, nameof(SizeChanged));

    #endregion Methods
}
