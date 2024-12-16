namespace Maui.DataGrid;

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls.Shapes;

/// <summary>
/// Specifies each column of the DataGrid.
/// </summary>
public sealed class DataGridColumn : BindableObject, IDefinition
{
    #region Bindable Properties

    /// <summary>
    /// Gets or sets the width of the column.
    /// </summary>
    public static readonly BindableProperty WidthProperty =
        BindablePropertyExtensions.Create<DataGridColumn, GridLength>(
            defaultValue: GridLength.Star,
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
        BindablePropertyExtensions.Create<DataGridColumn, string>(
            defaultValue: string.Empty,
            propertyChanged: (b, _, n) => ((DataGridColumn)b).HeaderLabel.Text = n);

    /// <summary>
    /// Gets or sets the filter text of the column.
    /// </summary>
    public static readonly BindableProperty FilterTextProperty =
        BindablePropertyExtensions.Create<DataGridColumn, string>(
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGridColumn self && self.DataGrid is not null)
                {
                    self.DataGrid.PageNumber = 1;
                    self.DataGrid.SortFilterAndPaginate();
                }
            });

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
        BindablePropertyExtensions.Create<DataGridColumn, bool>(
            defaultValue: true,
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGridColumn column)
                {
                    try
                    {
                        column.DataGrid?.Initialize();
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
        BindablePropertyExtensions.Create<DataGridColumn, bool>(
            defaultValue: true,
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGridColumn self)
                {
                    self.HeaderCell = null;
                    self.DataGrid?.Initialize();
                }
            });

    /// <summary>
    /// Gets or sets a value indicating whether filtering is enabled for the column.
    /// </summary>
    public static readonly BindableProperty FilteringEnabledProperty =
        BindablePropertyExtensions.Create<DataGridColumn, bool>(
            defaultValue: true,
            propertyChanged: (b, _, n) =>
            {
                if (b is DataGridColumn self)
                {
                    self.FilterTextbox.IsVisible = n;
                    self.DataGrid?.Initialize();
                }
            });

    /// <summary>
    /// Gets or sets the style for the header label of the column.
    /// </summary>
    public static readonly BindableProperty HeaderLabelStyleProperty =
        BindablePropertyExtensions.Create<DataGridColumn, Style>(
            propertyChanged: (b, _, n) =>
            {
                if (b is DataGridColumn self && self.HeaderLabel != null)
                {
                    if (n is null)
                    {
                        if (self.DataGrid is not null)
                        {
                            self.HeaderLabel.Style = self.DataGrid.DefaultHeaderLabelStyle;
                        }
                    }
                    else
                    {
                        self.HeaderLabel.Style = n;
                    }
                }
            });

    /// <summary>
    /// Gets or sets the style for the header filter of the column.
    /// </summary>
    public static readonly BindableProperty HeaderFilterStyleProperty =
        BindablePropertyExtensions.Create<DataGridColumn, Style>(
            propertyChanged: (b, _, n) =>
            {
                if (b is DataGridColumn self)
                {
                    if (n is null)
                    {
                        if (self.DataGrid is not null)
                        {
                            self.FilterTextbox.Style = self.DataGrid.DefaultHeaderFilterStyle;
                        }
                    }
                    else
                    {
                        self.FilterTextbox.Style = n;
                    }
                }
            });

    #endregion Bindable Properties

    #region Fields

    private readonly ColumnDefinition _invisibleColumnDefinition = new(0);
    private readonly WeakEventManager _sizeChangedEventManager = new();
    private readonly WeakEventManager _visibilityChangedEventManager = new();

    private bool? _isSortable;
    private ColumnDefinition? _columnDefinition;
    private TextAlignment? _verticalTextAlignment;
    private TextAlignment? _horizontalTextAlignment;

    #endregion Fields

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridColumn"/> class.
    /// </summary>
    public DataGridColumn()
    {
        SortingIconContainer = new ContentView
        {
            Content = SortingIcon,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false,
        };

#if NET9_0_OR_GREATER
        FilterTextbox.SetBinding(Entry.TextProperty, BindingBase.Create<DataGridColumn, string>(static x => x.FilterText, BindingMode.TwoWay, source: this));
#else
        FilterTextbox.SetBinding(Entry.TextProperty, new Binding(nameof(FilterText), BindingMode.TwoWay, source: this));
#endif
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

    #region Properties

    /// <summary>
    /// Gets or sets width of the column.
    /// Like Grid, you can use <see cref="GridUnitType.Absolute"/>, <see cref="GridUnitType.Star"/>, or <see cref="GridUnitType.Auto"/>.
    /// Be careful when using Auto. Columns may become misaligned.
    /// </summary>
    [TypeConverter(typeof(GridLengthTypeConverter))]
    public GridLength Width
    {
        get => (GridLength)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    /// <summary>
    /// Gets or sets column title.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets formatted title for column.
    /// <example>
    /// <code>
    /// <![CDATA[
    /// <DataGridColumn.FormattedTitle>
    ///     <FormattedString>
    ///       <Span Text="Home" TextColor="Black" FontSize="13" FontAttributes="Bold" />
    ///       <Span Text=" (won-lost)" TextColor="#333333" FontSize="11" />
    ///     </FormattedString>
    /// </DataGridColumn.FormattedTitle>
    /// ]]>
    /// </code>
    /// </example>
    /// </summary>
    public FormattedString FormattedTitle
    {
        get => (string)GetValue(FormattedTitleProperty);
        set => SetValue(FormattedTitleProperty, value);
    }

    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <summary>
    /// Gets or sets property name to bind in the object.
    /// </summary>
    public string PropertyName
    {
        get => (string)GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this column visible.
    /// </summary>
    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets string format for the cell.
    /// </summary>
    public string? StringFormat
    {
        get => (string?)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    /// <summary>
    /// Gets or sets cell template.
    /// Default value is <see cref="Label"/> with binding <see cref="PropertyName"/>.
    /// </summary>
    public DataTemplate? CellTemplate
    {
        get => (DataTemplate?)GetValue(CellTemplateProperty);
        set => SetValue(CellTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets edit cell template.
    /// Default value is <see cref="Entry"/> with binding <see cref="PropertyName"/>.
    /// </summary>
    public DataTemplate? EditCellTemplate
    {
        get => (DataTemplate?)GetValue(EditCellTemplateProperty);
        set => SetValue(EditCellTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets <see cref="LineBreakMode"/> for the text.
    /// Default value is <see cref="LineBreakMode.WordWrap"/>.
    /// </summary>
    public LineBreakMode LineBreakMode
    {
        get => (LineBreakMode)GetValue(LineBreakModeProperty);
        set => SetValue(LineBreakModeProperty, value);
    }

    /// <summary>
    /// Gets or sets horizontal alignment of the cell content.
    /// </summary>
    public LayoutOptions HorizontalContentAlignment
    {
        get => (LayoutOptions)GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets vertical alignment of the cell content.
    /// </summary>
    public LayoutOptions VerticalContentAlignment
    {
        get => (LayoutOptions)GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the column is sortable.
    /// Default is true. But the DataGrid must also have sorting enabled.
    /// Sortable columns must implement <see cref="IComparable"/>.
    /// </summary>
    public bool SortingEnabled
    {
        get => (bool)GetValue(SortingEnabledProperty);
        set => SetValue(SortingEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the column can be filtered.
    /// Default is true. But the DataGrid must also have filtering enabled.
    /// </summary>
    public bool FilteringEnabled
    {
        get => (bool)GetValue(FilteringEnabledProperty);
        set => SetValue(FilteringEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets label style of the header. <see cref="Style.TargetType"/> must be Label.
    /// </summary>
    public Style HeaderLabelStyle
    {
        get => (Style)GetValue(HeaderLabelStyleProperty);
        set => SetValue(HeaderLabelStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets label style of the header. <see cref="Style.TargetType"/> must be Label.
    /// </summary>
    public Style HeaderFilterStyle
    {
        get => (Style)GetValue(HeaderFilterStyleProperty);
        set => SetValue(HeaderFilterStyleProperty, value);
    }

    internal Polygon SortingIcon { get; } = new();

    internal Entry FilterTextbox { get; } = new() { Placeholder = "Filter" };

    internal ContentView FilterTextboxContainer { get; } = new();

    internal Label HeaderLabel { get; } = new();

    internal Grid HeaderLabelContainer { get; } = new()
    {
        ColumnDefinitions =
        [
            new() { Width = new(1, GridUnitType.Star) },
            new() { Width = new(1, GridUnitType.Auto) },
        ],
    };

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

    #endregion Properties

    #region Methods

    /// <summary>
    /// Determines via reflection if the column's data type is sortable.
    /// If you want to disable sorting for specific column please use <see cref="SortingEnabled"/> property.
    /// </summary>
    /// <returns>Boolean value representing whether the column is sortable.</returns>
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

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Reflection is needed here.")]
    [UnconditionalSuppressMessage("Trimming", "IL2062", Justification = "Reflection is needed here.")]
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
