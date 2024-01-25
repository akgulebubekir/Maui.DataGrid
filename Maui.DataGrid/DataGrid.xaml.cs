namespace Maui.DataGrid;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Font = Microsoft.Maui.Font;

/// <summary>
/// DataGrid component for .NET MAUI
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DataGrid
{
    #region Fields

    private static readonly SortedSet<int> DefaultPageSizeList = [5, 10, 50, 100, 200, 1000];

    private readonly WeakEventManager _itemSelectedEventManager = new();
    private readonly WeakEventManager _refreshingEventManager = new();

    private readonly SortedSet<int> _pageSizeList = new(DefaultPageSizeList);

    private readonly object _reloadLock = new();
    private readonly object _sortAndPaginateLock = new();
    private DataGridColumn? _sortedColumn;
    private HashSet<object>? _internalItemsHashSet;

    #endregion Fields

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGrid"/> class.
    /// </summary>
    public DataGrid()
    {
        InitializeComponent();
        if (_collectionView != null)
        {
            _collectionView.ItemsSource = InternalItems;
        }
    }

    #endregion ctor

    #region Events

    /// <summary>
    /// Occurs when an item is selected in the DataGrid.
    /// </summary>
    public event EventHandler<SelectionChangedEventArgs> ItemSelected
    {
        add => _itemSelectedEventManager.AddEventHandler(value);
        remove => _itemSelectedEventManager.RemoveEventHandler(value);
    }

    /// <summary>
    /// Occurs when the DataGrid is being refreshed.
    /// </summary>
    public event EventHandler Refreshing
    {
        add => _refreshingEventManager.AddEventHandler(value);
        remove => _refreshingEventManager.RemoveEventHandler(value);
    }

    #endregion Events

    #region Sorting methods

    private bool CanSort(SortData? sortData)
    {
        if (sortData is null)
        {
            Debug.WriteLine("No sort data");
            return false;
        }

        if (InternalItems.Count == 0)
        {
            Debug.WriteLine("There are no items to sort");
            return false;
        }

        if (!IsSortable)
        {
            Debug.WriteLine("DataGrid is not sortable");
            return false;
        }

        if (Columns.Count < 1)
        {
            Debug.WriteLine("There are no columns on this DataGrid.");
            return false;
        }

        if (sortData.Index >= Columns.Count)
        {
            Debug.WriteLine("Sort index is out of range");
            return false;
        }

        var columnToSort = Columns[sortData.Index];

        if (columnToSort.PropertyName == null)
        {
            Debug.WriteLine($"Please set the {nameof(columnToSort.PropertyName)} of the column");
            return false;
        }

        if (!columnToSort.SortingEnabled)
        {
            Debug.WriteLine($"{columnToSort.PropertyName} column does not have sorting enabled");
            return false;
        }

        if (!columnToSort.IsSortable())
        {
            Debug.WriteLine($"{columnToSort.PropertyName} column is not sortable");
            return false;
        }

        return true;
    }

    private IList<object> GetSortedItems(IList<object> unsortedItems, SortData sortData)
    {
        var columnToSort = Columns[sortData.Index];

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

        IEnumerable<object> items;

        switch (sortData.Order)
        {
            case SortingOrder.Ascendant:
                items = unsortedItems.OrderBy(x => x.GetValueByPath(columnToSort.PropertyName));
                _ = columnToSort.SortingIcon.RotateTo(0);
                break;
            case SortingOrder.Descendant:
                items = unsortedItems.OrderByDescending(x => x.GetValueByPath(columnToSort.PropertyName));
                _ = columnToSort.SortingIcon.RotateTo(180);
                break;
            case SortingOrder.None:
                return unsortedItems;
            default:
                throw new NotImplementedException();
        }

        return items.ToList();
    }

    #endregion Sorting methods

    #region Pagination methods

    private IEnumerable<object> GetPaginatedItems(IEnumerable<object> unpaginatedItems)
    {
        var skip = (PageNumber - 1) * PageSize;

        return unpaginatedItems.Skip(skip).Take(PageSize);
    }

    /// <summary>
    /// Checks if PageSizeList contains the new PageSize value, so that it shows in the dropdown
    /// </summary>
    private void UpdatePageSizeList()
    {
        if (PageSizeList.Contains(PageSize))
        {
            return;
        }

        if (_pageSizeList.Add(PageSize))
        {
            PageSizeList = new(_pageSizeList);
            OnPropertyChanged(nameof(PageSizeList));
            OnPropertyChanged(nameof(PageSize));
        }
    }

    private void SortAndPaginate(SortData? sortData = null)
    {
        lock (_sortAndPaginateLock)
        {
            if (ItemsSource is null)
            {
                return;
            }

            sortData ??= SortedColumnIndex;

            var originalItems = ItemsSource.Cast<object>().ToList();

            PageCount = (int)Math.Ceiling(originalItems.Count / (double)PageSize);

            if (originalItems.Count == 0)
            {
                InternalItems.Clear();
                return;
            }

            IList<object> sortedItems;

            if (sortData != null && CanSort(sortData))
            {
                sortedItems = GetSortedItems(originalItems, sortData);
            }
            else
            {
                sortedItems = originalItems;
            }

            if (PaginationEnabled)
            {
                var paginatedItems = GetPaginatedItems(sortedItems);
                InternalItems.ReplaceRange(paginatedItems);
            }
            else
            {
                InternalItems.ReplaceRange(sortedItems);
            }
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

    /// <summary>
    /// Gets or sets the color of the active row.
    /// </summary>
    public static readonly BindableProperty ActiveRowColorProperty =
        BindablePropertyExtensions.Create<DataGrid, Color>(Color.FromRgb(128, 144, 160),
            coerceValue: (b, v) =>
            {
                if (((DataGrid)b).SelectionMode == SelectionMode.None)
                {
                    throw new InvalidOperationException($"{nameof(SelectionMode)} for DataGrid cannot be None when attempting to set ActiveRowColor");
                }

                return v;
            });

    /// <summary>
    /// Gets or sets the background color of the header.
    /// </summary>
    public static readonly BindableProperty HeaderBackgroundProperty =
        BindablePropertyExtensions.Create<DataGrid, Color>(Colors.White,
            propertyChanged: (b, o, n) =>
            {
                var self = (DataGrid)b;
                if (o != n && self._headerRow != null && !self.HeaderBordersVisible)
                {
                    foreach (var cell in self._headerRow.Children.OfType<DataGridCell>())
                    {
                        cell.UpdateCellColors(n);
                    }
                }
            });

    /// <summary>
    /// Gets or sets the background color of the footer.
    /// </summary>
    public static readonly BindableProperty FooterBackgroundProperty =
        BindablePropertyExtensions.Create<DataGrid, Color>(Colors.White);

    /// <summary>
    /// Gets or sets the color of the border.
    /// </summary>
    public static readonly BindableProperty BorderColorProperty =
        BindablePropertyExtensions.Create<DataGrid, Color>(Colors.Black,
            propertyChanged: (b, _, _) =>
            {
                var self = (DataGrid)b;

                if (self._headerRow != null && self.HeaderBordersVisible)
                {
                    self._headerRow.InitializeHeaderRow();
                }

                self.Reload();
            });

    /// <summary>
    /// Gets or sets the ItemSizingStrategy for the data grid.
    /// </summary>
    public static readonly BindableProperty ItemSizingStrategyProperty =
        BindablePropertyExtensions.Create<DataGrid, ItemSizingStrategy>(DeviceInfo.Platform == DevicePlatform.Android ? ItemSizingStrategy.MeasureAllItems : ItemSizingStrategy.MeasureFirstItem);

    /// <summary>
    /// Gets or sets the row to edit.
    /// </summary>
    public static readonly BindableProperty RowToEditProperty =
        BindablePropertyExtensions.Create<DataGrid, object>();

    /// <summary>
    /// Gets or sets the background color palette for the rows.
    /// </summary>
    public static readonly BindableProperty RowsBackgroundColorPaletteProperty =
        BindablePropertyExtensions.Create<DataGrid, IColorProvider>(new PaletteCollection { Colors.White },
            propertyChanged: (b, _, _) =>
            {
                var self = (DataGrid)b;
                if (self.Columns != null && self.ItemsSource != null)
                {
                    self.Reload();
                }
            });

    /// <summary>
    /// Gets or sets the text color palette for the rows.
    /// </summary>
    public static readonly BindableProperty RowsTextColorPaletteProperty =
        BindablePropertyExtensions.Create<DataGrid, IColorProvider>(new PaletteCollection { Colors.Black },
            propertyChanged: (b, _, _) =>
            {
                var self = (DataGrid)b;
                if (self.Columns != null && self.ItemsSource != null)
                {
                    self.Reload();
                }
            });

    /// <summary>
    /// Gets or sets the Columns for the DataGrid.
    /// </summary>
    public static readonly BindableProperty ColumnsProperty =
        BindablePropertyExtensions.Create<DataGrid, ObservableCollection<DataGridColumn>>([],
            propertyChanged: (b, o, n) =>
            {
                if (n == o || b is not DataGrid self)
                {
                    return;
                }

                if (o != null)
                {
                    o.CollectionChanged -= self.OnColumnsChanged;

                    foreach (var oldColumn in o)
                    {
                        oldColumn.SizeChanged -= self.OnColumnSizeChanged;
                    }
                }

                if (n != null)
                {
                    n.CollectionChanged += self.OnColumnsChanged;

                    foreach (var newColumn in n)
                    {
                        newColumn.SizeChanged += self.OnColumnSizeChanged;
                    }
                }

                self.Reload();
            },
            defaultValueCreator: _ => []); // Note: defaultValueCreator needed to prevent errors during navigation

    /// <summary>
    /// Gets or sets the ItemsSource for the DataGrid.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty =
        BindablePropertyExtensions.Create<DataGrid, IEnumerable>(
            propertyChanged: (b, o, n) =>
            {
                if (n == o || b is not DataGrid self)
                {
                    return;
                }

                // Reset internal hash set, used for fast lookups
                self._internalItemsHashSet = null;

                // Unsubscribe from old collection's change event
                if (o is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= self.OnItemsSourceCollectionChanged;
                }

                // Subscribe to new collection's change event and update properties
                if (n is INotifyCollectionChanged newCollection)
                {
                    newCollection.CollectionChanged += self.OnItemsSourceCollectionChanged;
                }

                self._headerRow.InitializeHeaderRow(true);
                self.SortAndPaginate();

                // Reset SelectedItem if it's not in the new collection
                if (self.SelectedItem != null && !self.GetInternalItems().Contains(self.SelectedItem))
                {
                    self.SelectedItem = null;
                }
            });

    /// <summary>
    /// Gets or sets a value indicating whether pagination is enabled in the DataGrid.
    /// </summary>
    public static readonly BindableProperty PaginationEnabledProperty =
        BindablePropertyExtensions.Create<DataGrid, bool>(false,
            propertyChanged: (b, o, n) =>
            {
                if (o != n)
                {
                    var self = (DataGrid)b;
                    self.SortAndPaginate();
                }
            });

    /// <summary>
    /// Gets or sets the page count for the DataGrid.
    /// </summary>
    public static readonly BindableProperty PageCountProperty =
        BindablePropertyExtensions.Create<DataGrid, int>(1, BindingMode.OneWayToSource);

    /// <summary>
    /// Gets or sets the page size for the DataGrid.
    /// </summary>
    public static readonly BindableProperty PageSizeProperty =
        BindablePropertyExtensions.Create<DataGrid, int>(100, BindingMode.TwoWay,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    self.PageNumber = 1;
                    self.SortAndPaginate();
                    self.UpdatePageSizeList();
                }
            });

    /// <summary>
    /// Gets or sets the list of available page sizes for the DataGrid.
    /// </summary>
    public static readonly BindableProperty PageSizeListProperty =
        BindablePropertyExtensions.Create<DataGrid, ObservableCollection<int>>(new(DefaultPageSizeList),
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    self.UpdatePageSizeList();
                }
            });

    /// <summary>
    /// Gets or sets a value indicating whether the page size is visible in the DataGrid.
    /// </summary>
    public static readonly BindableProperty PageSizeVisibleProperty =
        BindablePropertyExtensions.Create<DataGrid, bool>(true);

    /// <summary>
    /// Gets or sets the list of available page sizes for the DataGrid.
    /// </summary>
    public static readonly BindableProperty PaginationStepperStyleProperty =
        BindablePropertyExtensions.Create<DataGrid, Style>(defaultValueCreator: x => x?.Resources["DefaultPaginationStepperStyle"] as Style);

    /// <summary>
    /// Gets or sets the row height for the DataGrid.
    /// </summary>
    public static readonly BindableProperty RowHeightProperty =
        BindablePropertyExtensions.Create<DataGrid, int>(40);

    /// <summary>
    /// Gets or sets the height of the footer in the DataGrid.
    /// </summary>
    public static readonly BindableProperty FooterHeightProperty =
        BindablePropertyExtensions.Create<DataGrid, int>(DeviceInfo.Platform == DevicePlatform.Android ? 50 : 40);

    /// <summary>
    /// Gets or sets the height of the header in the DataGrid.
    /// </summary>
    public static readonly BindableProperty HeaderHeightProperty =
        BindablePropertyExtensions.Create<DataGrid, int>(40);

    /// <summary>
    /// Gets or sets a value indicating whether the DataGrid is sortable.
    /// </summary>
    public static readonly BindableProperty IsSortableProperty =
        BindablePropertyExtensions.Create<DataGrid, bool>(true);

    /// <summary>
    /// Gets or sets the font size for the DataGrid.
    /// </summary>
    public static readonly BindableProperty FontSizeProperty =
        BindablePropertyExtensions.Create<DataGrid, double>(13.0);

    /// <summary>
    /// Gets or sets the font family for the DataGrid.
    /// </summary>
    public static readonly BindableProperty FontFamilyProperty =
        BindablePropertyExtensions.Create<DataGrid, string>(Font.Default.Family);

    /// <summary>
    /// Gets or sets the selected item in the DataGrid.
    /// </summary>
    public static readonly BindableProperty SelectedItemProperty =
        BindablePropertyExtensions.Create<DataGrid, object>(null, BindingMode.TwoWay,
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
                if (v is null || b is not DataGrid self || self.SelectionMode == SelectionMode.None)
                {
                    return null;
                }

                if (self.GetInternalItems().Contains(v))
                {
                    return v;
                }

                return null;
            }
        );

    /// <summary>
    /// Gets or sets the selected items in the DataGrid.
    /// </summary>
    public static readonly BindableProperty SelectedItemsProperty =
        BindablePropertyExtensions.Create<DataGrid, ObservableRangeCollection<object>>([], BindingMode.TwoWay,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (self._collectionView.SelectedItems != n)
                {
                    self._collectionView.SelectedItems = n;
                }
            },
            coerceValue: (b, v) =>
            {
                if (v is null || b is not DataGrid self || self.SelectionMode == SelectionMode.None)
                {
                    return null;
                }

                var internalItems = self.GetInternalItems(v.Count);

                foreach (var selectedItem in v)
                {
                    if (!internalItems.Contains(selectedItem))
                    {
                        return null;
                    }
                }

                return null;
            }
        );

    /// <summary>
    /// Gets or sets a value indicating whether selection is enabled in the DataGrid.
    /// </summary>
    public static readonly BindableProperty SelectionEnabledProperty =
        BindablePropertyExtensions.Create<DataGrid, bool>(true,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && !n)
                {
                    var self = (DataGrid)b;
                    self.SelectedItem = null;
                }
            });

    /// <summary>
    /// Gets or sets a value indicating whether selection is enabled in the DataGrid.
    /// </summary>
    public static readonly BindableProperty SelectionModeProperty =
        BindablePropertyExtensions.Create<DataGrid, SelectionMode>(SelectionMode.Single, BindingMode.TwoWay,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;

                switch (n)
                {
                    case SelectionMode.None:
                        self.SelectedItem = null;
                        self.SelectedItems.Clear();
                        break;
                    case SelectionMode.Single:
                        self.SelectedItems.Clear();
                        break;
                    case SelectionMode.Multiple:
                        self.SelectedItem = null;
                        break;
                }

                if (self._collectionView != null)
                {
                    self._collectionView.SelectionMode = n;
                }
            });

    /// <summary>
    /// Gets or sets a value indicating whether refreshing is enabled in the DataGrid.
    /// </summary>
    public static readonly BindableProperty RefreshingEnabledProperty =
        BindablePropertyExtensions.Create<DataGrid, bool>(true,
            propertyChanged: (b, o, n) =>
            {
                if (o != n)
                {
                    var self = (DataGrid)b;
                    _ = self.PullToRefreshCommand?.CanExecute(() => n);
                }
            });

    /// <summary>
    /// Gets or sets the command to execute when the data grid is pulled to refresh.
    /// </summary>
    public static readonly BindableProperty PullToRefreshCommandProperty =
        BindablePropertyExtensions.Create<DataGrid, ICommand>(
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
                    self._refreshView.Command = n;
                    _ = self._refreshView.Command?.CanExecute(self.RefreshingEnabled);
                }
            });

    /// <summary>
    /// Gets or sets the parameter to pass to the <see cref="PullToRefreshCommand"/>
    /// </summary>
    public static readonly BindableProperty PullToRefreshCommandParameterProperty =
        BindablePropertyExtensions.Create<DataGrid, object>();

    /// <summary>
    /// Gets or sets the spinner color to use while refreshing.
    /// </summary>
    public static readonly BindableProperty RefreshColorProperty =
        BindablePropertyExtensions.Create<DataGrid, Color>(Colors.Purple);

    /// <summary>
    /// Gets or sets a value indicating whether the DataGrid is refreshing.
    /// </summary>
    public static readonly BindableProperty IsRefreshingProperty =
        BindablePropertyExtensions.Create<DataGrid, bool>(false, BindingMode.TwoWay);

    /// <summary>
    /// Gets or sets the thickness of the border around the DataGrid.
    /// </summary>
    public static readonly BindableProperty BorderThicknessProperty =
        BindablePropertyExtensions.Create<DataGrid, Thickness>(new Thickness(1), BindingMode.TwoWay);

    /// <summary>
    /// Gets or sets a value indicating whether the header borders are visible in the DataGrid.
    /// </summary>
    public static readonly BindableProperty HeaderBordersVisibleProperty =
        BindablePropertyExtensions.Create<DataGrid, bool>(true,
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self._headerRow.InitializeHeaderRow();
                }
            });

    /// <summary>
    /// Gets or sets the index of the sorted column in the DataGrid.
    /// </summary>
    public static readonly BindableProperty SortedColumnIndexProperty =
        BindablePropertyExtensions.Create<DataGrid, SortData>(null, BindingMode.TwoWay,
            (b, v) =>
            {
                var self = (DataGrid)b;

                if (!self.IsLoaded)
                {
                    return true;
                }

                return self.CanSort(v);
            },
            (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    if (n != null && Math.Abs(n.Index) < self.Columns.Count)
                    {
                        self._sortedColumn = self.Columns[n.Index];
                    }

                    self.SortAndPaginate(n);
                }
            });

    /// <summary>
    /// Gets or sets the current page number in the DataGrid.
    /// </summary>
    public static readonly BindableProperty PageNumberProperty =
        BindablePropertyExtensions.Create<DataGrid, int>(1, BindingMode.TwoWay,
            (b, v) =>
            {
                if (b is DataGrid self)
                {
                    return v == 1 || v <= self.PageCount;
                }

                return false;
            },
            (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    self.SortAndPaginate();
                }
            });

    /// <summary>
    /// Gets or sets the style for the header labels in the DataGrid.
    /// </summary>
    public static readonly BindableProperty HeaderLabelStyleProperty =
        BindablePropertyExtensions.Create<DataGrid, Style>();

    /// <summary>
    /// Gets or sets the sort icons for the DataGrid.
    /// </summary>
    public static readonly BindableProperty SortIconProperty =
        BindablePropertyExtensions.Create<DataGrid, Polygon>();

    /// <summary>
    /// Gets or sets the style for the sort icons in the DataGrid.
    /// </summary>
    public static readonly BindableProperty SortIconStyleProperty =
        BindablePropertyExtensions.Create<DataGrid, Style>(
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    foreach (var column in self.Columns)
                    {
                        column.SortingIcon.Style = n;
                    }
                }
            });

    /// <summary>
    /// Gets or sets the view to be displayed when the DataGrid has no data.
    /// </summary>
    public static readonly BindableProperty NoDataViewProperty =
        BindablePropertyExtensions.Create<DataGrid, View>(
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    self._collectionView.EmptyView = n;
                }
            });

    #endregion Bindable properties

    #region Properties

#pragma warning disable CA2227 // Collection properties should be read only

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
    /// Default value is <see cref="Colors.White"/>
    /// </summary>
    public Color HeaderBackground
    {
        get => (Color)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>
    /// BackgroundColor of the footer that contains pagination elements
    /// Default value is <see cref="Colors.White"/>
    /// </summary>
    public Color FooterBackground
    {
        get => (Color)GetValue(FooterBackgroundProperty);
        set => SetValue(FooterBackgroundProperty, value);
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
    /// Default Value is <see cref="ItemSizingStrategy.MeasureFirstItem"/>, except on Android
    /// </summary>
    public ItemSizingStrategy ItemSizingStrategy
    {
        get => (ItemSizingStrategy)GetValue(ItemSizingStrategyProperty);
        set => SetValue(ItemSizingStrategyProperty, value);
    }

    /// <summary>
    /// The row to set to edit mode.
    /// </summary>
    public object RowToEdit
    {
        get => GetValue(RowToEditProperty);
        set => SetValue(RowToEditProperty, value);
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
    /// It does not sets header font size. Use <see cref="HeaderLabelStyle"/> to set header font size.
    /// </summary>
    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Sets the font family.
    /// It does not sets header font family. Use <see cref="HeaderLabelStyle"/> to set header font size.
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
    /// Gets or sets the list of available page sizes
    /// </summary>
    public ObservableCollection<int> PageSizeList
    {
        get => (ObservableCollection<int>)GetValue(PageSizeListProperty);
        set => SetValue(PageSizeListProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the page size picker is visible
    /// </summary>
    public bool PageSizeVisible
    {
        get => (bool)GetValue(PageSizeVisibleProperty);
        set => SetValue(PageSizeVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the pagination stepper style
    /// </summary>
    public Style? PaginationStepperStyle
    {
        get => (Style?)GetValue(PaginationStepperStyleProperty);
        set => SetValue(PaginationStepperStyleProperty, value);
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
    /// If you want to enable or disable sorting for specific column please use <see cref="DataGridColumn.SortingEnabled"/> property
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
    /// Set the SelectionMode for the DataGrid. Default value is Single
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
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
    /// Selected items
    /// </summary>
    public ObservableCollection<object> SelectedItems
    {
        get => (ObservableCollection<object>)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
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
    /// Gets or sets the parameter to pass to the <see cref="PullToRefreshCommand"/>
    /// </summary>
    public object PullToRefreshCommandParameter
    {
        get => GetValue(PullToRefreshCommandParameterProperty);
        set => SetValue(PullToRefreshCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the spinner color to use while refreshing.
    /// </summary>
    public Color RefreshColor
    {
        get => (Color)GetValue(RefreshColorProperty);
        set => SetValue(RefreshColorProperty, value);
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
    /// Border thickness for cells
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Determines to show the borders of header cells.
    /// Default value is true
    /// </summary>
    public bool HeaderBordersVisible
    {
        get => (bool)GetValue(HeaderBordersVisibleProperty);
        set => SetValue(HeaderBordersVisibleProperty, value);
    }

    /// <summary>
    /// Column index and sorting order for the DataGrid
    /// </summary>
    public SortData? SortedColumnIndex
    {
        get => (SortData?)GetValue(SortedColumnIndexProperty);
        set => SetValue(SortedColumnIndexProperty, value);
    }

    /// <summary>
    /// Style of the header label.
    /// Style's <see cref="Style.TargetType"/> must be Label.
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
    /// Style's <see cref="Style.TargetType"/> must be Polygon.
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
        get => (int)_paginationStepper.Maximum;
        private set
        {
            if (value > 0)
            {
                _paginationStepper.Maximum = value;
                _paginationStepper.IsEnabled = value > 1;
            }
        }
    }

#pragma warning restore CA2227 // Collection properties should be read only

    internal ObservableRangeCollection<object> InternalItems { get; } = [];

    #endregion Properties

    #region UI Methods

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent is null)
        {
            _collectionView.SelectionChanged -= OnSelectionChanged;
        }
        else
        {
            _collectionView.SelectionChanged -= OnSelectionChanged;
            _collectionView.SelectionChanged += OnSelectionChanged;
        }

        if (Parent is null)
        {
            _refreshView.Refreshing -= OnRefreshing;
        }
        else
        {
            _refreshView.Refreshing -= OnRefreshing;
            _refreshView.Refreshing += OnRefreshing;
        }

        if (Parent is null)
        {
            foreach (var column in Columns)
            {
                column.SizeChanged -= OnColumnSizeChanged;
            }

            Columns.CollectionChanged -= OnColumnsChanged;
        }
        else
        {
            foreach (var column in Columns)
            {
                column.SizeChanged -= OnColumnSizeChanged;
                column.SizeChanged += OnColumnSizeChanged;
            }

            Columns.CollectionChanged -= OnColumnsChanged;
            Columns.CollectionChanged += OnColumnsChanged;
        }
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        _headerRow.InitializeHeaderRow();
    }

    private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var newSortedColumnIndex = RegenerateSortedColumnIndex();

        if (newSortedColumnIndex != SortedColumnIndex)
        {
            // This will do a SortAndPaginate via the propertyChanged event of the SortedColumnIndexProperty
            SortedColumnIndex = newSortedColumnIndex;
        }

        Reload();
    }

    private void OnColumnSizeChanged(object? sender, EventArgs e) => Reload();

    private void OnRefreshing(object? sender, EventArgs e) => _refreshingEventManager.HandleEvent(this, e, nameof(Refreshing));

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);

        switch (SelectionMode)
        {
            case SelectionMode.Single:
                var collectionView = (CollectionView)sender;

                if (e.CurrentSelection.Count > 1)
                {
                    collectionView.SelectedItems.Clear();
                }

                SelectedItem = collectionView.SelectedItem;

                break;

            case SelectionMode.Multiple:
                var isChanged = false;

                if (e.CurrentSelection.Count == SelectedItems.Count)
                {
                    for (var i = 0; i < e.CurrentSelection.Count; i++)
                    {
                        if (SelectedItems[i] != e.CurrentSelection[i])
                        {
                            isChanged = true;
                        }
                    }
                }

                if (isChanged && SelectedItems is ObservableRangeCollection<object> selectedItems)
                {
                    selectedItems.ReplaceRange(e.CurrentSelection);
                }

                break;

            case SelectionMode.None:
                if (e.CurrentSelection.Count != 0)
                {
                    throw new InvalidOperationException("Item(s) selected when SelectionMode is None");
                }

                break;
        }

        _itemSelectedEventManager.HandleEvent(this, e, nameof(ItemSelected));
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SortAndPaginate();

        ICollection<object> internalItems;

        switch (SelectionMode)
        {
            case SelectionMode.Single:
                if (SelectedItem == null)
                {
                    break;
                }

                internalItems = GetInternalItems(SelectedItems.Count);

                if (!internalItems.Contains(SelectedItem))
                {
                    SelectedItem = null;
                }

                break;
            case SelectionMode.Multiple:
                if (SelectedItems == null)
                {
                    break;
                }

                internalItems = GetInternalItems(SelectedItems.Count);

                foreach (var selectedItem in SelectedItems)
                {
                    if (!internalItems.Contains(selectedItem))
                    {
                        SelectedItems.Clear();
                    }
                }

                break;
        }
    }

    private ICollection<object> GetInternalItems(int lookupCount = 1)
    {
        if (_internalItemsHashSet != null)
        {
            return _internalItemsHashSet;
        }

        if (lookupCount <= 1)
        {
            return InternalItems;
        }

        return _internalItemsHashSet = new HashSet<object>(InternalItems);
    }

    private SortData? RegenerateSortedColumnIndex()
    {
        if (_sortedColumn != null && SortedColumnIndex != null)
        {
            var newSortedColumnIndex = Columns.IndexOf(_sortedColumn);

            if (newSortedColumnIndex == -1)
            {
                return null;
            }

            return new(newSortedColumnIndex, SortedColumnIndex.Order);
        }

        return SortedColumnIndex;
    }

    internal void Reload()
    {
        if (!IsLoaded)
        {
            return;
        }

        lock (_reloadLock)
        {
            UpdatePageSizeList();

            _headerRow.InitializeHeaderRow();
        }
    }

    #endregion UI Methods
}
