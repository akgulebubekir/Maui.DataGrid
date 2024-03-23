namespace Maui.DataGrid;

using System.ComponentModel;
using System.Diagnostics;
using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls.Shapes;

/// <summary>
/// Specifies each column of the DataGrid.
/// </summary>
public sealed class DataGridColumn : BindableObject, IDefinition
{
    #region Fields

    private bool? _isSortable;
    private ColumnDefinition? _columnDefinition;
    private TextAlignment? _verticalTextAlignment;
    private TextAlignment? _horizontalTextAlignment;
    private readonly ColumnDefinition _invisibleColumnDefinition = new(0);
    private readonly WeakEventManager _sizeChangedEventManager = new();
    private readonly WeakEventManager _visibilityChangedEventManager = new();

    #endregion Fields

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridColumn"/> class.
    /// </summary>
    public DataGridColumn()
    {
        SortingIconContainer = new ContentView
        {
            IsVisible = false,
            Content = SortingIcon,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
    }

    #region Events

    /// <summary>
    /// Occurs when the size of the column changes.
    /// </summary>
    public event EventHandler SizeChanged
    {
        add => _sizeChangedEventManager.AddEventHandler(value);
        remove => _sizeChangedEventManager.RemoveEventHandler(value);
    }

    /// <summary>
    /// Occurs when the visibility of the column changes.
    /// </summary>
    public event EventHandler VisibilityChanged
    {
        add => _visibilityChangedEventManager.AddEventHandler(value);
        remove => _visibilityChangedEventManager.RemoveEventHandler(value);
    }

    #endregion Events

    #region Bindable Properties

    /// <summary>
    /// Gets or sets the width of the column.
    /// </summary>
    public static readonly BindableProperty WidthProperty =
        BindablePropertyExtensions.Create<DataGridColumn, GridLength>(GridLength.Star,
            propertyChanged: (b, o, n) =>
            {
                if (!o.Equals(n) && b is DataGridColumn self)
                {
                    if (self.ColumnDefinition == null)
                    {
                        self.ColumnDefinition = new(n);
                    }
                    else
                    {
                        self.ColumnDefinition.Width = n;
                    }

                    self.OnSizeChanged();
                }
            });

    /// <summary>
    /// Gets or sets the title of the column.
    /// </summary>
    public static readonly BindableProperty TitleProperty =
        BindablePropertyExtensions.Create<DataGridColumn, string>(string.Empty,
            propertyChanged: (b, _, n) => ((DataGridColumn)b).HeaderLabel.Text = n);

    /// <summary>
    /// Gets or sets the formatted title of the column.
    /// </summary>
    public static readonly BindableProperty FormattedTitleProperty =
        BindablePropertyExtensions.Create<DataGridColumn, FormattedString>(
            propertyChanged: (b, _, n) => ((DataGridColumn)b).HeaderLabel.FormattedText = n);

    /// <summary>
    /// Gets or sets the name of the property associated with the column.
    /// </summary>
    public static readonly BindableProperty PropertyNameProperty =
        BindablePropertyExtensions.Create<DataGridColumn, string>();

    /// <summary>
    /// Gets or sets a value indicating whether the column is visible.
    /// </summary>
    public static readonly BindableProperty IsVisibleProperty =
        BindablePropertyExtensions.Create<DataGridColumn, bool>(true,
            propertyChanged: (b, o, n) =>
            {
                if (b is DataGridColumn column)
                {
                    try
                    {
                        column.DataGrid?.Reload();
                    }
                    finally
                    {
                        column.OnVisibilityChanged();
                    }
                }
            });

    /// <summary>
    /// Gets or sets the string format for the column.
    /// </summary>
    public static readonly BindableProperty StringFormatProperty =
        BindablePropertyExtensions.Create<DataGridColumn, string>();

    /// <summary>
    /// Gets or sets the cell template for the column.
    /// </summary>
    public static readonly BindableProperty CellTemplateProperty =
        BindablePropertyExtensions.Create<DataGridColumn, DataTemplate>();

    /// <summary>
    /// Gets or sets the cell template for editing the column.
    /// </summary>
    public static readonly BindableProperty EditCellTemplateProperty =
        BindablePropertyExtensions.Create<DataGridColumn, DataTemplate>();

    /// <summary>
    /// Gets or sets the line break mode for the column.
    /// </summary>
    public static readonly BindableProperty LineBreakModeProperty =
        BindablePropertyExtensions.Create<DataGridColumn, LineBreakMode>(LineBreakMode.WordWrap);

    /// <summary>
    /// Gets or sets the horizontal content alignment for the column.
    /// </summary>
    public static readonly BindableProperty HorizontalContentAlignmentProperty =
        BindablePropertyExtensions.Create<DataGridColumn, LayoutOptions>(LayoutOptions.Center);

    /// <summary>
    /// Gets or sets the vertical content alignment for the column.
    /// </summary>
    public static readonly BindableProperty VerticalContentAlignmentProperty =
        BindablePropertyExtensions.Create<DataGridColumn, LayoutOptions>(LayoutOptions.Center);

    /// <summary>
    /// Gets or sets a value indicating whether sorting is enabled for the column.
    /// </summary>
    public static readonly BindableProperty SortingEnabledProperty =
        BindablePropertyExtensions.Create<DataGridColumn, bool>(true);

    /// <summary>
    /// Gets or sets the style for the header label of the column.
    /// </summary>
    public static readonly BindableProperty HeaderLabelStyleProperty =
        BindablePropertyExtensions.Create<DataGridColumn, Style>(
            propertyChanged: (b, o, n) =>
            {
                if (b is DataGridColumn self && self.HeaderLabel != null)
                {
                    self.HeaderLabel.Style = n;
                }
            });

    #endregion Bindable Properties

    #region Properties

    internal Polygon SortingIcon { get; } = new();

    internal Label HeaderLabel { get; } = new();

    internal View SortingIconContainer { get; }

    internal SortingOrder SortingOrder { get; set; }

    internal Type? DataType { get; private set; }

    internal DataGrid? DataGrid { get; set; }

    internal ColumnDefinition? ColumnDefinition
    {
        get => IsVisible ? _columnDefinition : _invisibleColumnDefinition;
        set => _columnDefinition = value;
    }

    internal DataGridCell? HeaderCell { get; set; }

    internal TextAlignment VerticalTextAlignment => _verticalTextAlignment ??= VerticalContentAlignment.ToTextAlignment();

    internal TextAlignment HorizontalTextAlignment => _horizontalTextAlignment ??= HorizontalContentAlignment.ToTextAlignment();

    /// <summary>
    /// Width of the column. Like Grid, you can use <see cref="GridUnitType.Absolute"/>, <see cref="GridUnitType.Star"/>, or <see cref="GridUnitType.Auto"/>.
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
    /// Cell template. Default value is <see cref="Label"/> with binding <see cref="PropertyName"/>
    /// </summary>
    public DataTemplate? CellTemplate
    {
        get => (DataTemplate?)GetValue(CellTemplateProperty);
        set => SetValue(CellTemplateProperty, value);
    }

    /// <summary>
    /// Edit cell template. Default value is <see cref="Entry"/> with binding <see cref="PropertyName"/>
    /// </summary>
    public DataTemplate? EditCellTemplate
    {
        get => (DataTemplate?)GetValue(EditCellTemplateProperty);
        set => SetValue(EditCellTemplateProperty, value);
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
    /// Label Style of the header. <see cref="Style.TargetType"/> must be Label.
    /// </summary>
    public Style HeaderLabelStyle
    {
        get => (Style)GetValue(HeaderLabelStyleProperty);
        set => SetValue(HeaderLabelStyleProperty, value);
    }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Determines via reflection if the column's data type is sortable.
    /// If you want to disable sorting for specific column please use <see cref="SortingEnabled"/> property
    /// </summary>
    public bool IsSortable()
    {
        if (_isSortable is not null)
        {
            return _isSortable.Value;
        }

        if (DataGrid?.ItemsSource is null)
        {
            _isSortable = false;
        }
        else if (DataType is not null)
        {
            _isSortable = typeof(IComparable).IsAssignableFrom(DataType);
        }

        return _isSortable ??= false;
    }

    internal void InitializeDataType()
    {
        if (DataType != null || string.IsNullOrEmpty(PropertyName))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(DataGrid);

        if (DataGrid.ItemsSource == null)
        {
            return;
        }

        try
        {
            Type? rowDataType = null;

            var genericArguments = DataGrid.ItemsSource.GetType().GetGenericArguments();

            if (genericArguments.Length == 1)
            {
                rowDataType = genericArguments[0];
            }
            else
            {
                var firstItem = DataGrid.ItemsSource.OfType<object>().FirstOrDefault(i => i != null);
                if (firstItem != default)
                {
                    rowDataType = firstItem.GetType();
                }
            }

            DataType = rowDataType?.GetPropertyTypeByPath(PropertyName);
        }
        catch (Exception ex)
            when (ex is NotSupportedException or ArgumentNullException or InvalidCastException)
        {
            Debug.WriteLine($"Attempting to obtain the data type for the column '{Title}' resulted in the following error: {ex.Message}");
        }
    }

    private void OnSizeChanged() => _sizeChangedEventManager.HandleEvent(this, EventArgs.Empty, nameof(SizeChanged));

    private void OnVisibilityChanged() => _visibilityChangedEventManager.HandleEvent(this, EventArgs.Empty, nameof(VisibilityChanged));

    #endregion Methods
}
