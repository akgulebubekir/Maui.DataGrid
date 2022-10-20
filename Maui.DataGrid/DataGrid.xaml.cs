using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Input;
using Maui.DataGrid.Utils;
using Font = Microsoft.Maui.Font;

namespace Maui.DataGrid;

/// <summary>
/// DataGrid component for Maui
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DataGrid
{
    private readonly Dictionary<int, SortingOrder> _sortingOrders;
    public event EventHandler Refreshing;
    public event EventHandler<SelectionChangedEventArgs> ItemSelected;

    #region ctor

    public DataGrid()
    {
        InitializeComponent();

        _sortingOrders = new Dictionary<int, SortingOrder>();

        _collectionView.SelectionChanged += (_, e) =>
        {
            if (SelectionEnabled)
            {
                SelectedItem = _collectionView.SelectedItem;
            }
            else
            {
                _collectionView.SelectedItem = null;
            }

            ItemSelected?.Invoke(this, e);
        };

        _refreshView.Refreshing += (_, e) => { Refreshing?.Invoke(this, e); };
    }

    #endregion

    #region Sorting methods

    internal void SortItems(SortData sortData)
    {
        if (InternalItems == null || sortData.Index >= Columns.Count || !Columns[sortData.Index].SortingEnabled)
        {
            return;
        }

        var items = InternalItems;
        var column = Columns[sortData.Index];
        var order = sortData.Order;

        if (!IsSortable)
        {
            throw new InvalidOperationException("DataGrid is not sortable");
        }

        if (column.PropertyName == null)
        {
            throw new InvalidOperationException("Please set 'PropertyName' of the Column");
        }

        //Sort
        items = order == SortingOrder.Descendant
            ? items.OrderByDescending(x => ReflectionUtils.GetValueByPath(x, column.PropertyName)).ToList()
            : items.OrderBy(x => ReflectionUtils.GetValueByPath(x, column.PropertyName)).ToList();

        column.SortingIcon.Style = order == SortingOrder.Descendant
            ? DescendingIconStyle ?? (Style)_headerView.Resources["DescendingIconStyle"]
            : AscendingIconStyle ?? (Style)_headerView.Resources["AscendingIconStyle"];

        //Support DescendingIcon property
        if (column.SortingIcon.Style.Setters.All(x => x.Property != Image.SourceProperty))
        {
            if (order == SortingOrder.Descendant && DescendingIconProperty.DefaultValue != DescendingIcon)
            {
                column.SortingIcon.Source = DescendingIcon;
            }

            if (order == SortingOrder.Ascendant && AscendingIconProperty.DefaultValue != AscendingIcon)
            {
                column.SortingIcon.Source = AscendingIcon;
            }
        }

        for (var i = 0; i < Columns.Count; i++)
        {
            if (i != sortData.Index)
            {
                if (Columns[i].SortingIcon.Style != null)
                {
                    Columns[i].SortingIcon.Style = null;
                }

                if (Columns[i].SortingIcon.Source != null)
                {
                    Columns[i].SortingIcon.Source = null;
                }

                _sortingOrders[i] = SortingOrder.None;
                Columns[i].SortingIcon.IsVisible = false;
            }
            else
            {
                Columns[i].SortingIcon.IsVisible = true;
            }
        }

        _internalItems = items;

        _sortingOrders[sortData.Index] = order;
        SortedColumnIndex = sortData;

        _collectionView.ItemsSource = _internalItems;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Scrolls to the row
    /// </summary>
    /// <param name="item">Item to scroll</param>
    /// <param name="position">Position of the row in screen</param>
    /// <param name="animated">animated</param>
    public void ScrollTo(object item, ScrollToPosition position, bool animated = true)
    {
        _collectionView.ScrollTo(item, position: position, animate: animated);
    }

    #endregion

    #region Bindable properties

    public static readonly BindableProperty ActiveRowColorProperty =
        BindableProperty.Create(nameof(ActiveRowColor), typeof(Color), typeof(DataGrid), Color.FromRgb(128, 144, 160),
            coerceValue: (b, v) =>
            {
                if (!((DataGrid)b).SelectionEnabled)
                {
                    throw new InvalidOperationException("Datagrid must be SelectionEnabled to set ActiveRowColor");
                }

                return v;
            });

    public static readonly BindableProperty HeaderBackgroundProperty =
        BindableProperty.Create(nameof(HeaderBackground), typeof(Color), typeof(DataGrid), Colors.White,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (self._headerView != null && !self.HeaderBordersVisible)
                {
                    self._headerView.BackgroundColor = (Color)n;
                }
            });

    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(DataGrid), Colors.Black,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (self.HeaderBordersVisible)
                {
                    self._headerView.BackgroundColor = (Color)n;
                }

                if (self.Columns != null && self.ItemsSource != null)
                {
                    self.Reload();
                }
            });

    public static readonly BindableProperty RowsBackgroundColorPaletteProperty =
        BindableProperty.Create(nameof(RowsBackgroundColorPalette), typeof(IColorProvider), typeof(DataGrid),
            new PaletteCollection
            {
                default
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
        BindableProperty.Create(nameof(Columns), typeof(ColumnCollection), typeof(DataGrid),
            propertyChanged: (b, _, _) => ((DataGrid)b).InitHeaderView(),
            defaultValueCreator: _ => new ColumnCollection());

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(DataGrid), null,
            propertyChanged: (b, o, n) =>
            {
                var self = (DataGrid)b;
                //ObservableCollection Tracking 
                if (o is INotifyCollectionChanged collectionChanged)
                {
                    collectionChanged.CollectionChanged -= self.HandleItemsSourceCollectionChanged;
                }

                if (n == null)
                {
                    self.InternalItems = null;
                }
                else
                {
                    if (n is INotifyCollectionChanged changed)
                    {
                        changed.CollectionChanged += self.HandleItemsSourceCollectionChanged;
                    }

                    self.InternalItems = new List<object>(((IEnumerable)n).Cast<object>());
                }

                if (self.SelectedItem != null && !self.InternalItems.Contains(self.SelectedItem))
                {
                    self.SelectedItem = null;
                }
            });

    private void HandleItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        InternalItems = new List<object>(((IEnumerable)sender).Cast<object>());
        if (SelectedItem != null && !InternalItems.Contains(SelectedItem))
        {
            SelectedItem = null;
        }
    }

    public static readonly BindableProperty RowHeightProperty =
        BindableProperty.Create(nameof(RowHeight), typeof(int), typeof(DataGrid), 40);

    public static readonly BindableProperty HeaderHeightProperty =
        BindableProperty.Create(nameof(HeaderHeight), typeof(int), typeof(DataGrid), 40,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                self._headerView.HeightRequest = (int)n;
            });

    public static readonly BindableProperty IsSortableProperty =
        BindableProperty.Create(nameof(IsSortable), typeof(bool), typeof(DataGrid), true);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(DataGrid), 13.0);

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(DataGrid), Font.Default.Family);

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(DataGrid), null, BindingMode.TwoWay,
            coerceValue: (b, v) =>
            {
                var self = (DataGrid)b;
                if (!self.SelectionEnabled && v != null)
                {
                    throw new InvalidOperationException("Datagrid must be SelectionEnabled=true to set SelectedItem");
                }

                if (self.InternalItems != null && self.InternalItems.Contains(v))
                {
                    return v;
                }

                return null;
            },
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (self._collectionView.SelectedItem != n)
                {
                    self._collectionView.SelectedItem = n;
                }
            }
        );

    public static readonly BindableProperty SelectionEnabledProperty =
        BindableProperty.Create(nameof(SelectionEnabled), typeof(bool), typeof(DataGrid), true,
            propertyChanged: (b, _, _) =>
            {
                var self = (DataGrid)b;
                if (!self.SelectionEnabled && self.SelectedItem != null)
                {
                    self.SelectedItem = null;
                }
            });

    public static readonly BindableProperty PullToRefreshCommandProperty =
        BindableProperty.Create(nameof(PullToRefreshCommand), typeof(ICommand), typeof(DataGrid), null,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (n == null)
                {
                    self._refreshView.IsEnabled = false;
                    self._refreshView.Command = null;
                }
                else
                {
                    self._refreshView.IsEnabled = true;
                    self._refreshView.Command = n as ICommand;
                }
            });

    public static readonly BindableProperty IsRefreshingProperty =
        BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(DataGrid), false, BindingMode.TwoWay,
            propertyChanged: (b, _, n) => ((DataGrid)b)._refreshView.IsRefreshing = (bool)n);

    public static readonly BindableProperty BorderThicknessProperty =
        BindableProperty.Create(nameof(BorderThickness), typeof(Thickness), typeof(DataGrid), new Thickness(1),
            propertyChanged: (b, _, n) =>
            {
                ((DataGrid)b)._headerView.ColumnSpacing = ((Thickness)n).HorizontalThickness / 2;
                ((DataGrid)b)._headerView.Padding = ((Thickness)n).HorizontalThickness / 2;
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
                var sData = (SortData)v;

                return
                    sData == null ||
                    self.Columns == null ||
                    self.Columns.Count == 0 ||
                    (sData.Index < self.Columns.Count && self.Columns.ElementAt(sData.Index).SortingEnabled);
            },
            (b, o, n) =>
            {
                var self = (DataGrid)b;
                if (o != n)
                {
                    self.SortItems((SortData)n);
                }
            });


    public static readonly BindableProperty HeaderLabelStyleProperty =
        BindableProperty.Create(nameof(HeaderLabelStyle), typeof(Style), typeof(DataGrid));

    public static readonly BindableProperty AscendingIconProperty =
        BindableProperty.Create(nameof(AscendingIcon), typeof(ImageSource), typeof(DataGrid),
            ImageSource.FromResource("Maui.DataGrid.up.png", typeof(DataGrid).GetTypeInfo().Assembly));

    public static readonly BindableProperty DescendingIconProperty =
        BindableProperty.Create(nameof(DescendingIcon), typeof(ImageSource), typeof(DataGrid),
            ImageSource.FromResource("Maui.DataGrid.down.png", typeof(DataGrid).GetTypeInfo().Assembly));

    public static readonly BindableProperty DescendingIconStyleProperty =
        BindableProperty.Create(nameof(DescendingIconStyle), typeof(Style), typeof(DataGrid), null,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                var style = ((Style)n).Setters.FirstOrDefault(x => x.Property == Image.SourceProperty);
                if (style != null)
                {
                    if (style.Value is string vs)
                    {
                        self.DescendingIcon = ImageSource.FromFile(vs);
                    }
                    else
                    {
                        self.DescendingIcon = (ImageSource)style.Value;
                    }
                }
            });

    public static readonly BindableProperty AscendingIconStyleProperty =
        BindableProperty.Create(nameof(AscendingIconStyle), typeof(Style), typeof(DataGrid), null,
            coerceValue: (_, v) => v,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (((Style)n).Setters.Any(x => x.Property == Image.SourceProperty))
                {
                    var style = ((Style)n).Setters.FirstOrDefault(x => x.Property == Image.SourceProperty);
                    if (style != null)
                    {
                        if (style.Value is string vs)
                        {
                            self.AscendingIcon = ImageSource.FromFile(vs);
                        }
                        else
                        {
                            self.AscendingIcon = (ImageSource)style.Value;
                        }
                    }
                }
            });

    public static readonly BindableProperty NoDataViewProperty =
        BindableProperty.Create(nameof(NoDataView), typeof(View), typeof(DataGrid),
            propertyChanged: (b, o, n) =>
            {
                if (o != n)
                {
                    ((DataGrid)b)._collectionView.EmptyView = n as View;
                }
            });

    #endregion

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

    private IList<object> _internalItems;

    internal IList<object> InternalItems
    {
        get => _internalItems;
        set
        {
            _internalItems = value;

            if (IsSortable && SortedColumnIndex != null)
            {
                SortItems(SortedColumnIndex);
            }
            else
            {
                _collectionView.ItemsSource = _internalItems;
            }
        }
    }

    /// <summary>
    /// Columns
    /// </summary>
    public ColumnCollection Columns
    {
        get => (ColumnCollection)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Font size of the cells.
    /// It does not sets header font size. Use <c>HeaderLabelStyle</c> to set header font size.
    /// </summary>
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Sets the font family.
    /// It does not sets header font family. Use <c>HeaderLabelStyle</c> to set header font size.
    /// 
    /// </summary>
    public string FontFamily
    {
        get => (string)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
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
    /// Sets header height
    /// </summary>
    public int HeaderHeight
    {
        get => (int)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    /// <summary>
    /// Determines if the grid is sortable. Default value is true.
    /// If you want to disable sorting for specific column please use <c>SortingEnabled</c> property
    /// </summary>
    public bool IsSortable
    {
        get => (bool)GetValue(IsSortableProperty);
        set => SetValue(IsSortableProperty, value);
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
    public object SelectedItem
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
    /// Border thickness for header &amp; each cell
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Determines to show the borders of header cells.
    /// Default value is <code>true</code>
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
    /// Ascending icon source 
    /// </summary>
    public ImageSource AscendingIcon
    {
        get => (ImageSource)GetValue(AscendingIconProperty);
        set => SetValue(AscendingIconProperty, value);
    }

    /// <summary>
    /// Descending icon source
    /// </summary>
    public ImageSource DescendingIcon
    {
        get => (ImageSource)GetValue(DescendingIconProperty);
        set => SetValue(DescendingIconProperty, value);
    }

    /// <summary>
    /// Style of the ascending icon
    /// Style's <c>TargetType</c> must be Image. 
    /// </summary>
    public Style AscendingIconStyle
    {
        get => (Style)GetValue(AscendingIconStyleProperty);
        set => SetValue(AscendingIconStyleProperty, value);
    }

    /// <summary>
    /// Style of the descending icon
    /// Style"s <c>TargetType</c> must be Image. 
    /// </summary>
    public Style DescendingIconStyle
    {
        get => (Style)GetValue(DescendingIconStyleProperty);
        set => SetValue(DescendingIconStyleProperty, value);
    }

    /// <summary>
    /// View to show when there is no data to display
    /// </summary>
    public View NoDataView
    {
        get => (View)GetValue(NoDataViewProperty);
        set => SetValue(NoDataViewProperty, value);
    }

    #endregion

    #region UI Methods

    protected override void OnParentSet()
    {
        base.OnParentSet();
        InitHeaderView();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        SetColumnsBindingContext();
    }

    private void Reload()
    {
        InternalItems = new List<object>(_internalItems);
    }

    #endregion

    #region Header Creation Methods

    private View GetHeaderViewForColumn(DataGridColumn column)
    {
        column.HeaderLabel.Style = column.HeaderLabelStyle ??
                                   HeaderLabelStyle ?? (Style)_headerView.Resources["HeaderDefaultStyle"];

        var grid = new Grid
        {
            ColumnSpacing = 0
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

        if (IsSortable)
        {
            column.SortingIcon.Style = (Style)_headerView.Resources["ImageStyleBase"];

            grid.Children.Add(column.SortingIcon);
            Grid.SetColumn(column.SortingIcon, 1);

            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (_, _) =>
            {
                var index = Columns.IndexOf(column);
                var order = _sortingOrders[index] == SortingOrder.Ascendant
                    ? SortingOrder.Descendant
                    : SortingOrder.Ascendant;

                if (Columns.ElementAt(index).SortingEnabled)
                {
                    SortedColumnIndex = new SortData(index, order);
                }
            };
            grid.GestureRecognizers.Add(tgr);
        }

        grid.Children.Add(column.HeaderLabel);

        return grid;
    }

    private void InitHeaderView()
    {
        SetColumnsBindingContext();
        _headerView.Children.Clear();
        _headerView.ColumnDefinitions.Clear();
        _sortingOrders.Clear();

        _headerView.Padding = new Thickness(BorderThickness.Left, BorderThickness.Top, BorderThickness.Right, 0);
        _headerView.ColumnSpacing = BorderThickness.HorizontalThickness / 2;

        if (Columns != null)
        {
            foreach (var col in Columns)
            {
                _headerView.ColumnDefinitions.Add(new ColumnDefinition { Width = col.Width });

                var cell = GetHeaderViewForColumn(col);

                _headerView.Children.Add(cell);
                Grid.SetColumn(cell, Columns.IndexOf(col));

                _sortingOrders.Add(Columns.IndexOf(col), SortingOrder.None);
            }
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

    #endregion
}
