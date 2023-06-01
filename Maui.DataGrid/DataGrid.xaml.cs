namespace Maui.DataGrid;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Maui.DataGrid.Utils;
using Microsoft.Maui.Controls.Shapes;
using Font = Microsoft.Maui.Font;

/// <summary>
/// DataGrid component for Maui
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DataGrid
{
    #region Fields

    private static readonly ColumnDefinitionCollection HeaderColumnDefinitions = new()
                {
                    new() { Width = new(1, GridUnitType.Star) },
                    new() { Width = new(1, GridUnitType.Auto) }
                };

    private readonly WeakEventManager _itemSelectedEventManager = new();
    private readonly WeakEventManager _refreshingEventManager = new();

    private readonly Style _defaultHeaderStyle;
    private readonly Style _defaultSortIconStyle;

    #endregion Fields

    #region ctor

    public DataGrid()
    {
        InitializeComponent();
        _defaultHeaderStyle = (Style)Resources["DefaultHeaderStyle"];
        _defaultSortIconStyle = (Style)Resources["DefaultSortIconStyle"];
    }

    #endregion ctor

    #region Events

    public event EventHandler<SelectionChangedEventArgs> ItemSelected
    {
        add => _itemSelectedEventManager.AddEventHandler(value);
        remove => _itemSelectedEventManager.RemoveEventHandler(value);
    }

    public event EventHandler Refreshing
    {
        add => _refreshingEventManager.AddEventHandler(value);
        remove => _refreshingEventManager.RemoveEventHandler(value);
    }

    #endregion Events

    #region Sorting methods

    private bool CanSort(SortData? sortData = null)
    {
        sortData ??= SortedColumnIndex;

        if (sortData is null)
        {
            Console.WriteLine("There is no sort data");
            return false;
        }

        if (InternalItems is null)
        {
            Console.WriteLine("There are no items to sort");
            return false;
        }

        if (!IsSortable)
        {
            Console.WriteLine("DataGrid is not sortable");
            return false;
        }

        if (Columns.Count < 1)
        {
            Console.WriteLine("There are no columns on this DataGrid.");
            return false;
        }

        if (sortData is null)
        {
            Console.WriteLine("Sort index is null");
            return false;
        }

        if (sortData.Index >= Columns.Count)
        {
            Console.WriteLine("Sort index is out of range");
            return false;
        }

        var columnToSort = Columns[sortData.Index];

        if (columnToSort.PropertyName == null)
        {
            Console.WriteLine($"Please set the {nameof(columnToSort.PropertyName)} of the column");
            return false;
        }

        if (!columnToSort.SortingEnabled)
        {
            Console.WriteLine($"{columnToSort.PropertyName} column does not have sorting enabled");
            return false;
        }

        if (!columnToSort.IsSortable(this))
        {
            Console.WriteLine($"{columnToSort.PropertyName} column is not sortable");
            return false;
        }

        return true;
    }

    private IList<object> GetSortedItems(IEnumerable<object> unsortedItems, SortData sortData)
    {
        var columnToSort = Columns[sortData.Index];

        IList<object> items;

        switch (sortData.Order)
        {
            case SortingOrder.Ascendant:
                items = unsortedItems.OrderBy(x => ReflectionUtils.GetValueByPath(x, columnToSort.PropertyName)).ToList();
                _ = columnToSort.SortingIcon.RotateTo(0);
                break;
            case SortingOrder.Descendant:
                items = unsortedItems.OrderByDescending(x => ReflectionUtils.GetValueByPath(x, columnToSort.PropertyName)).ToList();
                _ = columnToSort.SortingIcon.RotateTo(180);
                break;
            case SortingOrder.None:
                items = unsortedItems.ToList();
                break;
            default:
                throw new NotImplementedException();
        }

        foreach (var column in Columns)
        {
            if (column == columnToSort)
            {
                column.SortingOrder = sortData.Order;
                column.SortingIconContainer.IsVisible = true;
            }
            else
            {
                column.SortingOrder = SortingOrder.None;
                column.SortingIconContainer.IsVisible = false;
            }
        }

        return items;
    }

    #endregion Sorting methods

    #region Pagination methods

    private IEnumerable<object> GetPaginatedItems(IEnumerable<object> unpaginatedItems)
    {
        var skip = (PageNumber - 1) * PageSize;

        return unpaginatedItems.Skip(skip).Take(PageSize);
    }

    private void SortAndPaginate(SortData? sortData = null)
    {
        if (ItemsSource is null)
        {
            return;
        }

        sortData ??= SortedColumnIndex;

        var originalItems = ItemsSource.Cast<object>().ToList();

        IList<object> sortedItems;

        if (CanSort(sortData))
        {
            sortedItems = GetSortedItems(originalItems, sortData);
        }
        else
        {
            sortedItems = originalItems;
        }

        if (PaginationEnabled)
        {
            InternalItems = GetPaginatedItems(sortedItems).ToList();
        }
        else
        {
            InternalItems = sortedItems;
        }
    }

    #endregion Pagination methods

    #region Methods

    /// <summary>
    /// Scrolls to the row
    /// </summary>
    /// <param name="item">Item to scroll</param>
    /// <param name="position">Position of the row in screen</param>
    /// <param name="animated">animated</param>
    public void ScrollTo(object item, ScrollToPosition position, bool animated = true) => _collectionView.ScrollTo(item, position: position, animate: animated);

    #endregion Methods

    #region Bindable properties

    public static readonly BindableProperty ActiveRowColorProperty =
        BindableProperty.Create(nameof(ActiveRowColor), typeof(Color), typeof(DataGrid), Color.FromRgb(128, 144, 160),
            coerceValue: (b, v) =>
            {
                if (!((DataGrid)b).SelectionEnabled)
                {
                    throw new InvalidOperationException("DataGrid must have SelectionEnabled to set ActiveRowColor");
                }

                return v;
            });

    public static readonly BindableProperty HeaderBackgroundProperty =
        BindableProperty.Create(nameof(HeaderBackground), typeof(Color), typeof(DataGrid), Colors.White,
            propertyChanged: (b, o, n) =>
            {
                var self = (DataGrid)b;
                if (o != n && self._headerView != null && !self.HeaderBordersVisible && n is Color color)
                {
                    foreach (var child in self._headerView.Children.OfType<View>())
                    {
                        child.BackgroundColor = color;
                    }

                    self._footerView.BackgroundColor = color;
                }
            });

    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(DataGrid), Colors.Black,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (self.HeaderBordersVisible && n is Color color)
                {
                    self._headerView.BackgroundColor = color;
                }

                if (self.Columns != null && self.ItemsSource != null)
                {
                    self.Reload();
                }
            });

    public static readonly BindableProperty ItemSizingStrategyProperty =
    BindableProperty.Create(nameof(ItemSizingStrategy), typeof(ItemSizingStrategy), typeof(DataGrid), DeviceInfo.Platform == DevicePlatform.Android ? ItemSizingStrategy.MeasureAllItems : ItemSizingStrategy.MeasureFirstItem);

    public static readonly BindableProperty RowsBackgroundColorPaletteProperty =
        BindableProperty.Create(nameof(RowsBackgroundColorPalette), typeof(IColorProvider), typeof(DataGrid),
            new PaletteCollection
            {
                Colors.White
            },
            propertyChanged: (b, _, _) =>
            {
                var self = (DataGrid)b;
                if (self.Columns != null && self.ItemsSource != null)
                {
                    self.Reload();
                }
            });

    public static readonly BindableProperty RowsTextColorPaletteProperty =
        BindableProperty.Create(nameof(RowsTextColorPalette), typeof(IColorProvider), typeof(DataGrid),
            new PaletteCollection { Colors.Black },
            propertyChanged: (b, _, _) =>
            {
                var self = (DataGrid)b;
                if (self.Columns != null && self.ItemsSource != null)
                {
                    self.Reload();
                }
            });

    public static readonly BindableProperty ColumnsProperty =
        BindableProperty.Create(nameof(Columns), typeof(ObservableCollection<DataGridColumn>), typeof(DataGrid),
            propertyChanged: (b, o, n) =>
            {
                if (n == o || b is not DataGrid self)
                {
                    return;
                }

                if (o is ObservableCollection<DataGridColumn> oldColumns)
                {
                    oldColumns.CollectionChanged -= self.OnColumnsChanged;

                    foreach (var oldColumn in oldColumns)
                    {
                        oldColumn.SizeChanged -= self.OnColumnSizeChanged;
                    }
                }

                if (n is ObservableCollection<DataGridColumn> newColumns)
                {
                    newColumns.CollectionChanged += self.OnColumnsChanged;

                    foreach (var newColumn in newColumns)
                    {
                        newColumn.SizeChanged += self.OnColumnSizeChanged;
                    }
                }

                self.Reload();
            },
            defaultValueCreator: _ => new ObservableCollection<DataGridColumn>());

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(DataGrid), null,
            propertyChanged: (b, o, n) =>
            {
                if (n == o || b is not DataGrid self)
                {
                    return;
                }

                //ObservableCollection Tracking
                if (o is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= self.HandleItemsSourceCollectionChanged;
                }

                if (n == null)
                {
                    self.InternalItems = null;
                }
                else
                {
                    if (n is INotifyCollectionChanged newCollection)
                    {
                        newCollection.CollectionChanged += self.HandleItemsSourceCollectionChanged;
                    }

                    var itemsSource = ((IEnumerable)n).Cast<object>().ToList();

                    self.PageCount = (int)Math.Ceiling(itemsSource.Count / (double)self.PageSize);

                    self.SortAndPaginate();
                }

                if (self.SelectedItem != null && self.InternalItems?.Contains(self.SelectedItem) != true)
                {
                    self.SelectedItem = null;
                }
            });

    private void HandleItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is IEnumerable items)
        {
            InternalItems = items.Cast<object>().ToList();
            if (SelectedItem != null && !InternalItems.Contains(SelectedItem))
            {
                SelectedItem = null;
            }
        }
    }

    public static readonly BindableProperty PageCountProperty =
        BindableProperty.Create(nameof(PageCount), typeof(int), typeof(DataGrid), 1,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self && n is int pageCount && pageCount > 0)
                {
                    self._paginationStepper.Maximum = pageCount;
                }
            });

    public static readonly BindableProperty PageSizeProperty =
        BindableProperty.Create(nameof(PageSize), typeof(int), typeof(DataGrid), 100,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    self.SortAndPaginate();
                }
            });

    public static readonly BindableProperty RowHeightProperty =
        BindableProperty.Create(nameof(RowHeight), typeof(int), typeof(DataGrid), 40);

    public static readonly BindableProperty FooterHeightProperty =
        BindableProperty.Create(nameof(FooterHeight), typeof(int), typeof(DataGrid), 40);

    public static readonly BindableProperty HeaderHeightProperty =
        BindableProperty.Create(nameof(HeaderHeight), typeof(int), typeof(DataGrid), 40);

    public static readonly BindableProperty IsSortableProperty =
        BindableProperty.Create(nameof(IsSortable), typeof(bool), typeof(DataGrid), true);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(DataGrid), 13.0);

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(DataGrid), Font.Default.Family);

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(DataGrid), null, BindingMode.TwoWay,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (self._collectionView.SelectedItem != n)
                {
                    self._collectionView.SelectedItem = n;
                }
            },
            coerceValue: (b, v) =>
            {
                if (v is null || b is not DataGrid self)
                {
                    return null;
                }

                if (!self.SelectionEnabled)
                {
                    throw new InvalidOperationException("DataGrid must have SelectionEnabled=true to set SelectedItem");
                }

                if (self.InternalItems?.Contains(v) == true)
                {
                    return v;
                }

                return null;
            }
        );

    public static readonly BindableProperty PaginationEnabledProperty =
        BindableProperty.Create(nameof(PaginationEnabled), typeof(bool), typeof(DataGrid), false,
            propertyChanged: (b, o, n) =>
            {
                if (o != n)
                {
                    var self = (DataGrid)b;
                    self.SortAndPaginate();
                }
            });

    public static readonly BindableProperty SelectionEnabledProperty =
        BindableProperty.Create(nameof(SelectionEnabled), typeof(bool), typeof(DataGrid), true,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && n is bool selectionEnabled && !selectionEnabled)
                {
                    var self = (DataGrid)b;
                    self.SelectedItem = null;
                }
            });

    public static readonly BindableProperty RefreshingEnabledProperty =
    BindableProperty.Create(nameof(RefreshingEnabled), typeof(bool), typeof(DataGrid), true,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && n is bool refreshingEnabled)
                {
                    var self = (DataGrid)b;
                    _ = self.PullToRefreshCommand?.CanExecute(() => refreshingEnabled);
                }
            });

    public static readonly BindableProperty PullToRefreshCommandProperty =
        BindableProperty.Create(nameof(PullToRefreshCommand), typeof(ICommand), typeof(DataGrid), null,
            propertyChanged: (b, o, n) =>
            {
                if (o == n || b is not DataGrid self)
                {
                    return;
                }

                if (n == null)
                {
                    self._refreshView.Command = null;
                }
                else
                {
                    self._refreshView.Command = n as ICommand;
                    _ = self._refreshView.Command?.CanExecute(self.RefreshingEnabled);
                }
            });

    public static readonly BindableProperty IsRefreshingProperty =
        BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(DataGrid), false, BindingMode.TwoWay);

    public static readonly BindableProperty BorderThicknessProperty =
        BindableProperty.Create(nameof(BorderThickness), typeof(Thickness), typeof(DataGrid), new Thickness(1),
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self.Reload();
                }
            });

    public static readonly BindableProperty HeaderBordersVisibleProperty =
        BindableProperty.Create(nameof(HeaderBordersVisible), typeof(bool), typeof(DataGrid), true,
            propertyChanged: (b, _, n) => ((DataGrid)b)._headerView.BackgroundColor =
                (bool)n ? ((DataGrid)b).BorderColor : ((DataGrid)b).HeaderBackground);

    public static readonly BindableProperty SortedColumnIndexProperty =
        BindableProperty.Create(nameof(SortedColumnIndex), typeof(SortData), typeof(DataGrid), null, BindingMode.TwoWay,
            (b, v) =>
            {
                var self = (DataGrid)b;
                var sortData = (SortData)v;

                return self.CanSort(sortData);
            },
            (b, o, n) =>
            {
                if (o != n && b is DataGrid self && n is SortData sortData)
                {
                    self.SortAndPaginate(sortData);
                }
            });

    public static readonly BindableProperty PageNumberProperty =
        BindableProperty.Create(nameof(PageNumber), typeof(int), typeof(DataGrid), 1, BindingMode.TwoWay,
            (b, v) =>
            {
                if (b is DataGrid self && v is int pageNumber)
                {
                    return pageNumber == 1 || pageNumber <= self.PageCount;
                }

                return false;
            },
            (b, o, n) =>
            {
                if (o != n && b is DataGrid self && self.ItemsSource?.Cast<object>().Any() == true)
                {
                    self.SortAndPaginate();
                }
            });

    public static readonly BindableProperty HeaderLabelStyleProperty =
        BindableProperty.Create(nameof(HeaderLabelStyle), typeof(Style), typeof(DataGrid));

    public static readonly BindableProperty SortIconProperty =
        BindableProperty.Create(nameof(SortIcon), typeof(Polygon), typeof(DataGrid));

    public static readonly BindableProperty SortIconStyleProperty =
        BindableProperty.Create(nameof(SortIconStyle), typeof(Style), typeof(DataGrid), null,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self && n is Style style)
                {
                    foreach (var column in self.Columns)
                    {
                        column.SortingIcon.Style = style;
                    }
                }
            });

    public static readonly BindableProperty NoDataViewProperty =
        BindableProperty.Create(nameof(NoDataView), typeof(View), typeof(DataGrid),
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    self._collectionView.EmptyView = n as View;
                }
            });

    #endregion Bindable properties

    #region Properties

    /// <summary>
    /// Selected Row color
    /// </summary>
    public Color ActiveRowColor
    {
        get => (Color)GetValue(ActiveRowColorProperty);
        set => SetValue(ActiveRowColorProperty, value);
    }

    /// <summary>
    /// BackgroundColor of the column header
    /// Default value is White
    /// </summary>
    public Color HeaderBackground
    {
        get => (Color)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>
    /// Border color
    /// Default Value is Black
    /// </summary>
    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    /// <summary>
    /// ItemSizingStrategy
    /// Default Value is MeasureFirstItem, except on Android
    /// </summary>
    public ItemSizingStrategy ItemSizingStrategy
    {
        get => (ItemSizingStrategy)GetValue(ItemSizingStrategyProperty);
        set => SetValue(ItemSizingStrategyProperty, value);
    }

    /// <summary>
    /// Background color of the rows. It repeats colors consecutively for rows.
    /// </summary>
    public IColorProvider RowsBackgroundColorPalette
    {
        get => (IColorProvider)GetValue(RowsBackgroundColorPaletteProperty);
        set => SetValue(RowsBackgroundColorPaletteProperty, value);
    }

    /// <summary>
    /// Text color of the rows. It repeats colors consecutively for rows.
    /// </summary>
    public IColorProvider RowsTextColorPalette
    {
        get => (IColorProvider)GetValue(RowsTextColorPaletteProperty);
        set => SetValue(RowsTextColorPaletteProperty, value);
    }

    /// <summary>
    /// ItemsSource of the DataGrid
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private IList<object>? _internalItems;

    internal IList<object>? InternalItems
    {
        get => _internalItems;
        set
        {
            _internalItems = value;

            _collectionView.ItemsSource = _internalItems; // TODO: Use efficent CollectionChanged handling with observables
        }
    }

    /// <summary>
    /// Columns
    /// </summary>
    public ObservableCollection<DataGridColumn> Columns
    {
        get => (ObservableCollection<DataGridColumn>)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Font size of the cells.
    /// It does not sets header font size. Use <c>HeaderLabelStyle</c> to set header font size.
    /// </summary>
    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Sets the font family.
    /// It does not sets header font family. Use <c>HeaderLabelStyle</c> to set header font size.
    /// </summary>
    public string FontFamily
    {
        get => (string)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>
    /// Sets the row height
    /// </summary>
    public int RowHeight
    {
        get => (int)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets footer height
    /// </summary>
    public int FooterHeight
    {
        get => (int)GetValue(FooterHeightProperty);
        set => SetValue(FooterHeightProperty, value);
    }

    /// <summary>
    /// Sets header height
    /// </summary>
    public int HeaderHeight
    {
        get => (int)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets if the grid is sortable. Default value is true.
    /// Sortable columns must implement <see cref="IComparable"/>
    /// If you want to enable or disable sorting for specific column please use <c>SortingEnabled</c> property
    /// </summary>
    public bool IsSortable
    {
        get => (bool)GetValue(IsSortableProperty);
        set => SetValue(IsSortableProperty, value);
    }

    /// <summary>
    /// Gets or sets the page number. Default value is 1
    /// </summary>
    public int PageNumber
    {
        get => (int)GetValue(PageNumberProperty);
        set => SetValue(PageNumberProperty, value);
    }

    /// <summary>
    /// Enables pagination in dataGrid. Default value is False
    /// </summary>
    public bool PaginationEnabled
    {
        get => (bool)GetValue(PaginationEnabledProperty);
        set => SetValue(PaginationEnabledProperty, value);
    }

    /// <summary>
    /// Enables selection in dataGrid. Default value is True
    /// </summary>
    public bool SelectionEnabled
    {
        get => (bool)GetValue(SelectionEnabledProperty);
        set => SetValue(SelectionEnabledProperty, value);
    }

    /// <summary>
    /// Selected item
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Executes the command when refreshing via pull
    /// </summary>
    public ICommand PullToRefreshCommand
    {
        get => (ICommand)GetValue(PullToRefreshCommandProperty);
        set => SetValue(PullToRefreshCommandProperty, value);
    }

    /// <summary>
    /// Displays an ActivityIndicator when is refreshing
    /// </summary>
    public bool IsRefreshing
    {
        get => (bool)GetValue(IsRefreshingProperty);
        set => SetValue(IsRefreshingProperty, value);
    }

    /// <summary>
    /// Enables refreshing the DataGrid by a pull down command
    /// </summary>
    public bool RefreshingEnabled
    {
        get => (bool)GetValue(RefreshingEnabledProperty);
        set => SetValue(RefreshingEnabledProperty, value);
    }

    /// <summary>
    /// Border thickness for header &amp; each cell
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Determines to show the borders of header cells.
    /// Default value is <c>true</c>
    /// </summary>
    public bool HeaderBordersVisible
    {
        get => (bool)GetValue(HeaderBordersVisibleProperty);
        set => SetValue(HeaderBordersVisibleProperty, value);
    }

    /// <summary>
    /// Column index and sorting order for the DataGrid
    /// </summary>
    public SortData SortedColumnIndex
    {
        get => (SortData)GetValue(SortedColumnIndexProperty);
        set => SetValue(SortedColumnIndexProperty, value);
    }

    /// <summary>
    /// Style of the header label.
    /// Style's <c>TargetType</c> must be Label.
    /// </summary>
    public Style HeaderLabelStyle
    {
        get => (Style)GetValue(HeaderLabelStyleProperty);
        set => SetValue(HeaderLabelStyleProperty, value);
    }

    /// <summary>
    /// Sort icon
    /// </summary>
    public Polygon SortIcon
    {
        get => (Polygon)GetValue(SortIconProperty);
        set => SetValue(SortIconProperty, value);
    }

    /// <summary>
    /// Style of the sort icon
    /// Style's <c>TargetType</c> must be Polygon.
    /// </summary>
    public Style SortIconStyle
    {
        get => (Style)GetValue(SortIconStyleProperty);
        set => SetValue(SortIconStyleProperty, value);
    }

    /// <summary>
    /// View to show when there is no data to display
    /// </summary>
    public View NoDataView
    {
        get => (View)GetValue(NoDataViewProperty);
        set => SetValue(NoDataViewProperty, value);
    }

    /// <summary>
    /// Gets the page count
    /// </summary>
    public int PageCount
    {
        get => (int)GetValue(PageCountProperty);
        private set => SetValue(PageCountProperty, value);
    }

    #endregion Properties

    #region UI Methods

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (SelectionEnabled)
        {
            if (Parent is null)
            {
                _collectionView.SelectionChanged -= OnSelectionChanged;
            }
            else
            {
                _collectionView.SelectionChanged += OnSelectionChanged;
            }
        }

        if (RefreshingEnabled)
        {
            if (Parent is null)
            {
                _refreshView.Refreshing -= OnRefreshing;
            }
            else
            {
                _refreshView.Refreshing += OnRefreshing;
            }
        }

        if (Parent is null)
        {
            Columns.CollectionChanged -= OnColumnsChanged;

            foreach (var column in Columns)
            {
                column.SizeChanged -= OnColumnSizeChanged;
            }
        }
        else
        {
            Columns.CollectionChanged += OnColumnsChanged;

            foreach (var column in Columns)
            {
                column.SizeChanged += OnColumnSizeChanged;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        InitHeaderView();
    }

    private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Reload();

    private void OnColumnSizeChanged(object? sender, EventArgs e) => Reload();

    private void OnRefreshing(object? sender, EventArgs e) => _refreshingEventManager.HandleEvent(this, e, nameof(Refreshing));

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedItem = _collectionView.SelectedItem;

        _itemSelectedEventManager.HandleEvent(this, e, nameof(ItemSelected));
    }

    internal void Reload()
    {
        InitHeaderView();

        if (_internalItems is not null)
        {
            InternalItems = new List<object>(_internalItems);
        }
    }

    #endregion UI Methods

    #region Header Creation Methods

    private View GetHeaderViewForColumn(DataGridColumn column, int index)
    {
        column.HeaderLabel.Style = column.HeaderLabelStyle ?? HeaderLabelStyle ?? _defaultHeaderStyle;

        if (IsSortable && column.SortingEnabled && column.IsSortable(this))
        {
            column.SortingIcon.Style = SortIconStyle ?? _defaultSortIconStyle;
            column.SortingIconContainer.HeightRequest = HeaderHeight * 0.3;
            column.SortingIconContainer.WidthRequest = HeaderHeight * 0.3;

            var grid = new Grid
            {
                ColumnSpacing = 0,
                Padding = new(0, 0, 4, 0),
                ColumnDefinitions = HeaderColumnDefinitions,
                Children = { column.HeaderLabel, column.SortingIconContainer },
                GestureRecognizers =
                {
                    new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            // This is to invert SortOrder when the user taps on a column.
                            var order = column.SortingOrder == SortingOrder.Ascendant
                                ? SortingOrder.Descendant
                                : SortingOrder.Ascendant;

                            SortedColumnIndex = new(index, order);
                        }, () => column.SortingEnabled)
                    }
                }
            };

            Grid.SetColumn(column.SortingIconContainer, 1);
            return grid;
        }

        return new ContentView
        {
            Content = column.HeaderLabel
        };
    }

    private void InitHeaderView()
    {
        SetColumnsBindingContext();

        _headerView.Children.Clear();
        _headerView.ColumnDefinitions.Clear();
        ResetSortingOrders();

        _headerView.Padding = new(BorderThickness.Left, BorderThickness.Top, BorderThickness.Right, 0);
        _headerView.ColumnSpacing = BorderThickness.HorizontalThickness;

        if (Columns == null)
        {
            return;
        }

        for (var i = 0; i < Columns.Count; i++)
        {
            var col = Columns[i];

            col.ColumnDefinition ??= new(col.Width);

            _headerView.ColumnDefinitions.Add(col.ColumnDefinition);

            if (!col.IsVisible)
            {
                continue;
            }

            col.HeaderView ??= GetHeaderViewForColumn(col, i);

            col.HeaderView.SetBinding(BackgroundColorProperty, new Binding(nameof(HeaderBackground), source: this));

            Grid.SetColumn(col.HeaderView, i);
            _headerView.Children.Add(col.HeaderView);
        }
    }

    private void ResetSortingOrders()
    {
        foreach (var column in Columns)
        {
            column.SortingOrder = SortingOrder.None;
        }
    }

    private void SetColumnsBindingContext()
    {
        if (Columns != null)
        {
            foreach (var c in Columns)
            {
                c.BindingContext = BindingContext;
            }
        }
    }

    #endregion Header Creation Methods
}
