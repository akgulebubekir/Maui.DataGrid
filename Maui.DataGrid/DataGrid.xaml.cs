namespace Maui.DataGrid;

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Maui.DataGrid.Collections;
using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Font = Microsoft.Maui.Font;

#pragma warning disable CA1724

/// <summary>
/// DataGrid component for .NET MAUI.
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DataGrid
{
    /// <summary>
    /// Gets or sets the color of the active row.
    /// </summary>
    public static readonly BindableProperty ActiveRowColorProperty =
        BindablePropertyExtensions.Create<DataGrid, Color>(Color.FromRgb(128, 144, 160));

    /// <summary>
    /// Gets or sets the background color of the header.
    /// </summary>
    public static readonly BindableProperty HeaderBackgroundProperty =
        BindablePropertyExtensions.Create<DataGrid, Color>(
            defaultValue: Colors.White,
            propertyChanged: (b, _, n) =>
            {
                if (b is DataGrid self && self._headerRow != null && !self.HeaderBordersVisible)
                {
                    foreach (var child in self._headerRow.Children)
                    {
                        if (child is DataGridCell cell)
                        {
                            cell.UpdateCellBackgroundColor(n);
                        }
                    }
                }
            });

    /// <summary>
    /// Gets or sets the Row Tapped Command.
    /// </summary>
    public static readonly BindableProperty RowTappedCommandProperty =
        BindablePropertyExtensions.Create<DataGrid, ICommand>();

    /// <summary>
    /// Gets or sets the background color of the footer.
    /// </summary>
    public static readonly BindableProperty FooterBackgroundProperty =
        BindablePropertyExtensions.Create<DataGrid, Color>(Colors.White);

    /// <summary>
    /// Gets or sets the text color of the footer.
    /// </summary>
    public static readonly BindableProperty FooterTextColorProperty =
        BindablePropertyExtensions.Create<DataGrid, Color>(Colors.Black);

    /// <summary>
    /// Gets or sets the color of the border.
    /// </summary>
    public static readonly BindableProperty BorderColorProperty =
        BindablePropertyExtensions.Create<DataGrid, Color>(
            defaultValue: Colors.Black,
            propertyChanged: (b, _, _) =>
            {
                var self = (DataGrid)b;

                if (self._headerRow != null && self.HeaderBordersVisible)
                {
                    self._headerRow.InitializeHeaderRow();
                }
            });

    /// <summary>
    /// Gets or sets the ItemSizingStrategy for the data grid.
    /// </summary>
    public static readonly BindableProperty ItemSizingStrategyProperty =
        BindablePropertyExtensions.Create<DataGrid, ItemSizingStrategy>(ItemSizingStrategy.MeasureFirstItem);

    /// <summary>
    /// Gets or sets the row to edit.
    /// </summary>
    public static readonly BindableProperty RowToEditProperty =
        BindablePropertyExtensions.Create<DataGrid, object>();

    /// <summary>
    /// Gets or sets the background color palette for the rows.
    /// </summary>
    public static readonly BindableProperty RowsBackgroundColorPaletteProperty =
        BindablePropertyExtensions.Create<DataGrid, IColorProvider>(
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self._rowsBackgroundColorPaletteChangedEventManager.HandleEvent(self, EventArgs.Empty, nameof(RowsBackgroundColorPaletteChanged));
                }
            },
            defaultValueCreator: _ => new PaletteCollection { Colors.White });

    /// <summary>
    /// Gets or sets the text color palette for the rows.
    /// </summary>
    public static readonly BindableProperty RowsTextColorPaletteProperty =
        BindablePropertyExtensions.Create<DataGrid, IColorProvider>(
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self._rowsTextColorPaletteChangedEventManager.HandleEvent(self, EventArgs.Empty, nameof(RowsTextColorPaletteChanged));
                }
            },
            defaultValueCreator: _ => new PaletteCollection { Colors.Black });

    /// <summary>
    /// Gets or sets the Columns for the DataGrid.
    /// </summary>
    public static readonly BindableProperty ColumnsProperty =
        BindablePropertyExtensions.Create<DataGrid, ObservableCollection<DataGridColumn>>(
            propertyChanged: (b, o, n) =>
            {
                if (b is not DataGrid self)
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

                self.Initialize();
            },
            defaultValueCreator: _ => []); // Note: defaultValueCreator needed to prevent errors during navigation

    /// <summary>
    /// Gets or sets the ItemsSource for the DataGrid.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty =
        BindablePropertyExtensions.Create<DataGrid, IEnumerable>(
            propertyChanged: (b, o, n) =>
            {
                if (b is not DataGrid self)
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
                self.SortFilterAndPaginate();

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
        BindablePropertyExtensions.Create<DataGrid, bool>(
            defaultValue: false,
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self.SortFilterAndPaginate();
                }
            });

    /// <summary>
    /// Gets or sets the text for the page label in the DataGrid.
    /// </summary>
    public static readonly BindableProperty PageTextProperty =
        BindablePropertyExtensions.Create<DataGrid, string>(
            defaultValue: "Page:",
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self.OnPropertyChanged(nameof(PageText));
                }
            });

    /// <summary>
    /// Gets or sets the localized text for the per page label.
    /// </summary>
    public static readonly BindableProperty PerPageTextProperty =
        BindablePropertyExtensions.Create<DataGrid, string>(
            defaultValue: "# per page:",
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self.OnPropertyChanged(nameof(PerPageText));
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
        BindablePropertyExtensions.Create<DataGrid, int>(
            defaultValue: 100,
            BindingMode.TwoWay,
            validateValue: (_, v) => v > 0,
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self.PageNumber = 1;
                    self.SortFilterAndPaginate();
                    self.UpdatePageSizeList();
                }
            });

    /// <summary>
    /// Gets or sets the list of available page sizes for the DataGrid.
    /// </summary>
    public static readonly BindableProperty PageSizeListProperty =
        BindablePropertyExtensions.Create<DataGrid, IList<int>>(
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self.UpdatePageSizeList();
                }
            },
            defaultValueCreator: _ => [.. DefaultPageSizeSet!]);

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
    /// Gets or sets a value indicating whether the DataGrid allows sorting.
    /// </summary>
    public static readonly BindableProperty SortingEnabledProperty =
        BindablePropertyExtensions.Create<DataGrid, bool>(
            defaultValue: true,
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self._headerRow.InitializeHeaderRow(true);
                }
            });

    /// <summary>
    /// Gets or sets a value indicating whether the DataGrid allows filtering.
    /// </summary>
    public static readonly BindableProperty FilteringEnabledProperty =
        BindablePropertyExtensions.Create<DataGrid, bool>(
            defaultValue: false,
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self._headerRow.InitializeHeaderRow(true);
                }
            });

    /// <summary>
    /// Obsolete. Use <see cref="SortingEnabledProperty"/> instead.
    /// </summary>
    [Obsolete("IsSortableProperty is obsolete. Please use SortingEnabledProperty instead.")]
    public static readonly BindableProperty IsSortableProperty = SortingEnabledProperty;

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
        BindablePropertyExtensions.Create<DataGrid, object>(
            defaultValue: null,
            BindingMode.TwoWay,
            propertyChanged: (b, _, n) =>
            {
                if (b is DataGrid self && self._collectionView.SelectedItem != n)
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
            });

    /// <summary>
    /// Gets or sets the selected items in the DataGrid.
    /// </summary>
    public static readonly BindableProperty SelectedItemsProperty =
        BindablePropertyExtensions.Create<DataGrid, IList<object>>(
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;

                if (self._collectionView != null && self._collectionView.SelectedItems != n)
                {
                    self._collectionView.SelectedItems = n;
                }
            },
            coerceValue: (b, v) =>
            {
                if (b is not DataGrid self)
                {
                    throw new InvalidOperationException("SelectedItems can only be set on a DataGrid");
                }

                if (v is null || self.SelectionMode == SelectionMode.None)
                {
                    self.SelectedItems.Clear();
                    return self.SelectedItems;
                }

                if (v is not IList<object> selectedItems)
                {
                    throw new InvalidCastException($"{nameof(SelectedItems)} must be of type IList<object>");
                }

                var internalItems = self.GetInternalItems(v.Count);

                foreach (var selectedItem in selectedItems)
                {
                    if (!internalItems.Contains(selectedItem))
                    {
                        _ = selectedItems.Remove(selectedItem);
                    }
                }

                return selectedItems;
            },
            defaultValueCreator: _ => []);

    /// <summary>
    /// Gets or sets a value indicating whether selection is enabled in the DataGrid.
    /// Default value is true.
    /// </summary>
    [Obsolete($"SelectionEnabled is obsolete. Please use {nameof(SelectionMode)} instead.")]
    public static readonly BindableProperty SelectionEnabledProperty =
        BindablePropertyExtensions.Create<DataGrid, bool>(
            defaultValue: true,
            propertyChanged: (b, _, n) =>
            {
                if (!n && b is DataGrid self)
                {
                    self.SelectedItem = null;
                    self.SelectedItems.Clear();

                    if (self.SelectionMode != SelectionMode.None)
                    {
                        self.SelectionMode = SelectionMode.None;
                    }
                }
            });

    /// <summary>
    /// Gets or sets a value indicating whether selection is enabled in the DataGrid.
    /// </summary>
    public static readonly BindableProperty SelectionModeProperty =
        BindablePropertyExtensions.Create<DataGrid, SelectionMode>(
            defaultValue: SelectionMode.Single,
            BindingMode.TwoWay,
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
        BindablePropertyExtensions.Create<DataGrid, bool>(
            defaultValue: true,
            propertyChanged: (b, _, n) =>
            {
                if (b is not DataGrid self)
                {
                    return;
                }

                if (self.PullToRefreshCommand?.CanExecute(n) != true)
                {
                    Debug.WriteLine("RefreshView cannot be executed.");
                }
            });

    /// <summary>
    /// Gets or sets the command to execute when the data grid is pulled to refresh.
    /// </summary>
    public static readonly BindableProperty PullToRefreshCommandProperty =
        BindablePropertyExtensions.Create<DataGrid, ICommand>(
            propertyChanged: (b, _, n) =>
            {
                if (b is not DataGrid self)
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
                    if (!self._refreshView.Command.CanExecute(self.RefreshingEnabled))
                    {
                        Debug.WriteLine("RefreshView cannot be executed.");
                    }
                }
            });

    /// <summary>
    /// Gets or sets the parameter to pass to the <see cref="PullToRefreshCommand"/>.
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
        BindablePropertyExtensions.Create<DataGrid, bool>(
            defaultValue: true,
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
        BindablePropertyExtensions.Create<DataGrid, SortData?>(
            defaultValue: null,
            BindingMode.TwoWay,
            validateValue: (b, v) =>
            {
                var self = (DataGrid)b;

                if (!self.IsLoaded || self.Columns == null)
                {
                    return true;
                }

                return self.CanSort(v);
            },
            propertyChanged: (b, _, n) =>
            {
                if (b is DataGrid self)
                {
                    if (n != null && n.Index < self.Columns.Count)
                    {
                        self._sortedColumn = self.Columns[n.Index];
                    }

                    self.SortFilterAndPaginate(n);
                }
            });

    /// <summary>
    /// Gets or sets the current page number in the DataGrid.
    /// </summary>
    public static readonly BindableProperty PageNumberProperty =
        BindablePropertyExtensions.Create<DataGrid, int>(
            defaultValue: 1,
            BindingMode.TwoWay,
            validateValue: (b, v) =>
            {
                if (v < 0)
                {
                    return false;
                }
                else if (b is DataGrid self)
                {
                    return v == 1 || v <= self.PageCount;
                }

                return false;
            },
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self.SortFilterAndPaginate();
                }
            });

    /// <summary>
    /// Gets or sets the style for the header labels in the DataGrid.
    /// </summary>
    public static readonly BindableProperty HeaderLabelStyleProperty =
        BindablePropertyExtensions.Create<DataGrid, Style>();

    /// <summary>
    /// Gets or sets the style for the column filters in the DataGrid.
    /// </summary>
    public static readonly BindableProperty HeaderFilterStyleProperty =
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
            propertyChanged: (b, _, n) =>
            {
                if (b is DataGrid self)
                {
                    foreach (var column in self.Columns)
                    {
                        if (n is null)
                        {
                            column.SortingIcon.Style = self.DefaultSortIconStyle;
                        }
                        else
                        {
                            column.SortingIcon.Style = n;
                        }
                    }
                }
            });

    /// <summary>
    /// Gets or sets the view to be displayed when the DataGrid has no data.
    /// </summary>
    public static readonly BindableProperty NoDataViewProperty =
        BindablePropertyExtensions.Create<DataGrid, View>(
            propertyChanged: (b, _, n) =>
            {
                if (b is DataGrid self)
                {
                    self._collectionView.EmptyView = n;
                }
            });

    private static readonly SortedSet<int> DefaultPageSizeSet = [5, 10, 50, 100, 200, 1000];

    private readonly WeakEventManager _itemSelectedEventManager = new();
    private readonly WeakEventManager _refreshingEventManager = new();
    private readonly WeakEventManager _rowsBackgroundColorPaletteChangedEventManager = new();
    private readonly WeakEventManager _rowsTextColorPaletteChangedEventManager = new();

    private readonly SortedSet<int> _pageSizeList = [.. DefaultPageSizeSet];

    private readonly ConcurrentDictionary<string, PropertyInfo?> _propertyCache = [];

#if NET9_0
    private readonly Lock _reloadLock = new();
    private readonly Lock _sortAndPaginateLock = new();
#else
    private readonly object _reloadLock = new();
    private readonly object _sortAndPaginateLock = new();
#endif
    private DataGridColumn? _sortedColumn;
    private HashSet<object>? _internalItemsHashSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGrid"/> class.
    /// </summary>
    public DataGrid()
    {
        InitializeComponent();

        DefaultHeaderLabelStyle = (Style)Resources["DefaultHeaderLabelStyle"];
        DefaultHeaderFilterStyle = (Style)Resources["DefaultHeaderFilterStyle"];
        DefaultSortIconStyle = (Style)Resources["DefaultSortIconStyle"];

        if (_collectionView != null)
        {
            _collectionView.ItemsSource = InternalItems;
        }
    }

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

    /// <summary>
    /// Occurs when the RowsBackgroundColorPalette of the DataGrid is changed.
    /// </summary>
    internal event EventHandler RowsBackgroundColorPaletteChanged
    {
        add => _rowsBackgroundColorPaletteChangedEventManager.AddEventHandler(value);
        remove => _rowsBackgroundColorPaletteChangedEventManager.RemoveEventHandler(value);
    }

    /// <summary>
    /// Occurs when the RowsTextColorPalette of the DataGrid is changed.
    /// </summary>
    internal event EventHandler RowsTextColorPaletteChanged
    {
        add => _rowsTextColorPaletteChangedEventManager.AddEventHandler(value);
        remove => _rowsTextColorPaletteChangedEventManager.RemoveEventHandler(value);
    }

#pragma warning disable CA2227 // Collection properties should be read only

    /// <summary>
    /// Gets or sets selected Row color.
    /// </summary>
    public Color ActiveRowColor
    {
        get => (Color)GetValue(ActiveRowColorProperty);
        set => SetValue(ActiveRowColorProperty, value);
    }

    /// <summary>
    /// Gets or sets backgroundColor of the column header
    /// Default value is <see cref="Colors.White"/>.
    /// </summary>
    public Color HeaderBackground
    {
        get => (Color)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets backgroundColor of the footer that contains pagination elements
    /// Default value is <see cref="Colors.White"/>.
    /// </summary>
    public Color FooterBackground
    {
        get => (Color)GetValue(FooterBackgroundProperty);
        set => SetValue(FooterBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets textColor of the footer that contains pagination elements
    /// Default value is <see cref="Colors.Black"/>.
    /// </summary>
    public Color FooterTextColor
    {
        get => (Color)GetValue(FooterTextColorProperty);
        set => SetValue(FooterTextColorProperty, value);
    }

    /// <summary>
    /// Gets or sets border color
    /// Default Value is Black.
    /// </summary>
    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    /// <summary>
    /// Gets or sets <see cref="ItemSizingStrategy"/>
    /// Default Value is <see cref="ItemSizingStrategy.MeasureFirstItem"/>.
    /// </summary>
    public ItemSizingStrategy ItemSizingStrategy
    {
        get => (ItemSizingStrategy)GetValue(ItemSizingStrategyProperty);
        set => SetValue(ItemSizingStrategyProperty, value);
    }

    /// <summary>
    /// Gets or sets the row to set to edit mode.
    /// </summary>
    public object RowToEdit
    {
        get => GetValue(RowToEditProperty);
        set => SetValue(RowToEditProperty, value);
    }

    /// <summary>
    /// Gets or sets background color of the rows. It repeats colors consecutively for rows.
    /// </summary>
    public IColorProvider RowsBackgroundColorPalette
    {
        get => (IColorProvider)GetValue(RowsBackgroundColorPaletteProperty);
        set => SetValue(RowsBackgroundColorPaletteProperty, value);
    }

    /// <summary>
    /// Gets or sets text color of the rows. It repeats colors consecutively for rows.
    /// </summary>
    public IColorProvider RowsTextColorPalette
    {
        get => (IColorProvider)GetValue(RowsTextColorPaletteProperty);
        set => SetValue(RowsTextColorPaletteProperty, value);
    }

    /// <summary>
    /// Gets or sets executes the command when a row is tapped. Works with selection disabled.
    /// </summary>
    public ICommand RowTappedCommand
    {
        get => (ICommand)GetValue(RowTappedCommandProperty);
        set => SetValue(RowTappedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets ItemsSource of the DataGrid.
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets columns of the DataGrid.
    /// </summary>
    public ObservableCollection<DataGridColumn> Columns
    {
        get => (ObservableCollection<DataGridColumn>)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets font size of the cells.
    /// It does not sets header font size. Use <see cref="HeaderLabelStyle"/> to set header font size.
    /// </summary>
    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the font family.
    /// It does not sets header font family. Use <see cref="HeaderLabelStyle"/> to set header font size.
    /// </summary>
    public string FontFamily
    {
        get => (string)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the list of available page sizes.
    /// </summary>
    public IList<int> PageSizeList
    {
        get => (IList<int>)GetValue(PageSizeListProperty);
        set => SetValue(PageSizeListProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the page size picker is visible.
    /// </summary>
    public bool PageSizeVisible
    {
        get => (bool)GetValue(PageSizeVisibleProperty);
        set => SetValue(PageSizeVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the pagination stepper style.
    /// </summary>
    public Style? PaginationStepperStyle
    {
        get => (Style?)GetValue(PaginationStepperStyleProperty);
        set => SetValue(PaginationStepperStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the row height.
    /// </summary>
    public int RowHeight
    {
        get => (int)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets footer height.
    /// </summary>
    public int FooterHeight
    {
        get => (int)GetValue(FooterHeightProperty);
        set => SetValue(FooterHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets header height.
    /// </summary>
    public int HeaderHeight
    {
        get => (int)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether sorting is enabled.
    /// Obsolete. Use <see cref="SortingEnabled"/> instead.
    /// </summary>
    [Obsolete("IsSortable is obsolete. Please use SortingEnabled instead.")]
    public bool IsSortable
    {
        get => (bool)GetValue(SortingEnabledProperty);
        set => SetValue(SortingEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets if the grid allows sorting. Default value is true.
    /// Sortable columns must implement <see cref="IComparable"/>
    /// If you want to enable or disable sorting for specific column please use <see cref="DataGridColumn.SortingEnabled"/> property.
    /// </summary>
    public bool SortingEnabled
    {
        get => (bool)GetValue(SortingEnabledProperty);
        set => SetValue(SortingEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets if the grid allows filtering. Default value is false.
    /// If you want to enable or disable filtering for specific column please use <see cref="DataGridColumn.FilteringEnabled"/> property.
    /// </summary>
    public bool FilteringEnabled
    {
        get => (bool)GetValue(FilteringEnabledProperty);
        set => SetValue(FilteringEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the page number. Default value is 1.
    /// </summary>
    public int PageNumber
    {
        get => (int)GetValue(PageNumberProperty);
        set => SetValue(PageNumberProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether pagination is enabled in the DataGrid.
    /// Default value is False.
    /// </summary>
    public bool PaginationEnabled
    {
        get => (bool)GetValue(PaginationEnabledProperty);
        set => SetValue(PaginationEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether sets whether selection is enabled for the DataGrid.
    /// Default value is true.
    /// </summary>
    [Obsolete($"SelectionEnabled is obsolete. Please use {nameof(SelectionMode)} instead.")]
    public bool SelectionEnabled
    {
        get => (bool)GetValue(SelectionEnabledProperty);
        set => SetValue(SelectionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets set the SelectionMode for the DataGrid.
    /// Default value is Single.
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected items.
    /// </summary>
    public IList<object> SelectedItems
    {
        get => (IList<object>)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when refreshing via a pull gesture.
    /// </summary>
    public ICommand PullToRefreshCommand
    {
        get => (ICommand)GetValue(PullToRefreshCommandProperty);
        set => SetValue(PullToRefreshCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the <see cref="PullToRefreshCommand"/>.
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
    /// Gets or sets a value indicating whether to display an ActivityIndicator.
    /// </summary>
    public bool IsRefreshing
    {
        get => (bool)GetValue(IsRefreshingProperty);
        set => SetValue(IsRefreshingProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether refreshing the DataGrid by a pull down command is enabled.
    /// </summary>
    public bool RefreshingEnabled
    {
        get => (bool)GetValue(RefreshingEnabledProperty);
        set => SetValue(RefreshingEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets border thickness for cells.
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the borders of header cells.
    /// Default value is true.
    /// </summary>
    public bool HeaderBordersVisible
    {
        get => (bool)GetValue(HeaderBordersVisibleProperty);
        set => SetValue(HeaderBordersVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets column index and sorting order for the DataGrid.
    /// </summary>
    public SortData? SortedColumnIndex
    {
        get => (SortData?)GetValue(SortedColumnIndexProperty);
        set => SetValue(SortedColumnIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets style of the header label.
    /// Style's <see cref="Style.TargetType"/> must be Label.
    /// </summary>
    public Style HeaderLabelStyle
    {
        get => (Style)GetValue(HeaderLabelStyleProperty);
        set => SetValue(HeaderLabelStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets style of the header label.
    /// Style's <see cref="Style.TargetType"/> must be Label.
    /// </summary>
    public Style HeaderFilterStyle
    {
        get => (Style)GetValue(HeaderFilterStyleProperty);
        set => SetValue(HeaderFilterStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets sort icon.
    /// </summary>
    public Polygon SortIcon
    {
        get => (Polygon)GetValue(SortIconProperty);
        set => SetValue(SortIconProperty, value);
    }

    /// <summary>
    /// Gets or sets style of the sort icon
    /// Style's <see cref="Style.TargetType"/> must be Polygon.
    /// </summary>
    public Style SortIconStyle
    {
        get => (Style)GetValue(SortIconStyleProperty);
        set => SetValue(SortIconStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets view to show when there is no data to display.
    /// </summary>
    public View NoDataView
    {
        get => (View)GetValue(NoDataViewProperty);
        set => SetValue(NoDataViewProperty, value);
    }

    /// <summary>
    /// Gets the page count.
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

            if (PageNumber > value)
            {
                PageNumber = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the customized text for the 'Page' label in the pagination section.
    /// </summary>
    public string PageText
    {
        get => (string)GetValue(PageTextProperty);
        set => SetValue(PageTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the customized text for the 'Per Page' label in the pagination section.
    /// </summary>
    public string PerPageText
    {
        get => (string)GetValue(PerPageTextProperty);
        set => SetValue(PerPageTextProperty, value);
    }

    internal Style DefaultHeaderLabelStyle { get; }

    internal Style DefaultHeaderFilterStyle { get; }

    internal Style DefaultSortIconStyle { get; }

    internal ObservableRangeCollection<object> InternalItems { get; } = [];

    /// <summary>
    /// Scrolls to the row.
    /// </summary>
    /// <param name="item">Item to scroll.</param>
    /// <param name="position">Position of the row in screen.</param>
    /// <param name="animated">animated.</param>
    public void ScrollTo(object item, ScrollToPosition position, bool animated = true) => _collectionView.ScrollTo(item, position: position, animate: animated);

    internal void Initialize()
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

    internal void SortFilterAndPaginate(SortData? sortData = null)
    {
        if (ItemsSource is null)
        {
            return;
        }

        lock (_sortAndPaginateLock)
        {
            sortData ??= SortedColumnIndex;

            var originalItems = ItemsSource as IList<object> ?? [.. ItemsSource.Cast<object>()];

            PageCount = (int)Math.Ceiling(originalItems.Count / (double)PageSize);

            if (originalItems.Count == 0)
            {
                InternalItems.Clear();
                return;
            }

            var filteredItems = CanFilter() ? GetFilteredItems(originalItems) : originalItems;

            var sortedItems = CanSort(sortData) ? GetSortedItems(filteredItems, sortData!) : filteredItems;

            var paginatedItems = PaginationEnabled ? GetPaginatedItems(sortedItems) : sortedItems;

            InternalItems.ReplaceRange(paginatedItems);
        }
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent is null)
        {
            Loaded -= OnLoaded;
        }
        else
        {
            Loaded -= OnLoaded;
            Loaded += OnLoaded;
        }

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

    private void OnLoaded(object? sender, EventArgs e) => Initialize();

    private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var newSortedColumnIndex = RegenerateSortedColumnIndex();

        if (newSortedColumnIndex != SortedColumnIndex)
        {
            // This will do a SortAndPaginate via the propertyChanged event of the SortedColumnIndexProperty
            SortedColumnIndex = newSortedColumnIndex;
        }

        Initialize();
    }

    private void OnColumnSizeChanged(object? sender, EventArgs e) => Initialize();

    private void OnRefreshing(object? sender, EventArgs e) => _refreshingEventManager.HandleEvent(this, e, nameof(Refreshing));

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _itemSelectedEventManager.HandleEvent(this, e, nameof(ItemSelected));
        RowTappedCommand?.Execute(e);
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _internalItemsHashSet = null;
        SortFilterAndPaginate();
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

        return _internalItemsHashSet = [.. InternalItems];
    }

    private SortData? RegenerateSortedColumnIndex()
    {
        if (_sortedColumn == null || SortedColumnIndex == null)
        {
            return SortedColumnIndex;
        }

        var newSortedColumnIndex = Columns.IndexOf(_sortedColumn);

        if (newSortedColumnIndex == -1)
        {
            return null;
        }

        return new(newSortedColumnIndex, SortedColumnIndex.Order);
    }

    private bool CanFilter() => FilteringEnabled && Columns.Any(c => c.FilteringEnabled);

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

        if (!SortingEnabled)
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

    private IEnumerable<object> GetSortedItems(IList<object> unsortedItems, SortData sortData)
    {
        _sortedColumn ??= Columns[sortData.Index];

        foreach (var column in Columns)
        {
            if (column == _sortedColumn)
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
                _ = _sortedColumn.SortingIcon.RotateTo(0);
                items = unsortedItems.OrderBy(x => x.GetValueByPath(_sortedColumn.PropertyName));
                break;
            case SortingOrder.Descendant:
                _ = _sortedColumn.SortingIcon.RotateTo(180);
                items = unsortedItems.OrderByDescending(x => x.GetValueByPath(_sortedColumn.PropertyName));
                break;
            case SortingOrder.None:
                return unsortedItems;
            default:
                throw new NotImplementedException();
        }

        return items;
    }

    private IList<object> GetFilteredItems(IList<object> originalItems)
    {
        var filteredItems = originalItems.AsEnumerable();

        foreach (var column in Columns)
        {
            if (!column.FilteringEnabled || string.IsNullOrEmpty(column.FilterText))
            {
                continue;
            }

            filteredItems = filteredItems.Where(item => FilterItem(item, column));
        }

        return [.. filteredItems];
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Reflection is needed here.")]
    private bool FilterItem(object item, DataGridColumn column)
    {
        try
        {
            if (string.IsNullOrEmpty(column.FilterText))
            {
                return true;
            }

            var itemType = item.GetType();
            var cacheKey = $"{itemType.FullName}|{column.PropertyName}";

            if (!_propertyCache.TryGetValue(cacheKey, out var property))
            {
                property = itemType.GetProperty(column.PropertyName);
                _propertyCache[cacheKey] = property;
            }

            if (property == null || property.PropertyType == typeof(object))
            {
                return false;
            }

            var value = property.GetValue(item)?.ToString();
            return value?.Contains(column.FilterText, StringComparison.OrdinalIgnoreCase) == true;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private IEnumerable<object> GetPaginatedItems(IEnumerable<object> unpaginatedItems)
    {
        var skip = (PageNumber - 1) * PageSize;

        return unpaginatedItems.Skip(skip).Take(PageSize);
    }

    /// <summary>
    /// Checks if PageSizeList contains the new PageSize value, so that it shows in the dropdown.
    /// </summary>
    private void UpdatePageSizeList()
    {
        if (_pageSizeList.Contains(PageSize))
        {
            return;
        }

        if (_pageSizeList.Add(PageSize))
        {
            PageSizeList = [.. _pageSizeList];
            OnPropertyChanged(nameof(PageSizeList));
            OnPropertyChanged(nameof(PageSize));
        }
    }
}
