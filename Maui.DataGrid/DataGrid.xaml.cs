namespace Maui.DataGrid;
using System.Collections;
using System.Collections.Specialized;
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
    private readonly Dictionary<int, SortingOrder> _sortingOrders;

    private readonly WeakEventManager _itemSelectedEventManager = new();

    public event EventHandler<SelectionChangedEventArgs> ItemSelected
    {
        add => this._itemSelectedEventManager.AddEventHandler(value);
        remove => this._itemSelectedEventManager.RemoveEventHandler(value);
    }

    private readonly WeakEventManager _refreshingEventManager = new();

    public event EventHandler Refreshing
    {
        add => this._refreshingEventManager.AddEventHandler(value);
        remove => this._refreshingEventManager.RemoveEventHandler(value);
    }

    #region ctor

    public DataGrid()
    {
        this.InitializeComponent();

        this._sortingOrders = new();
    }

    #endregion ctor

    #region Sorting methods

    private void SortItems(SortData sortData)
    {
        if (this.InternalItems == null || sortData.Index >= this.Columns.Count || !this.Columns[sortData.Index].SortingEnabled)
        {
            return;
        }

        var column = this.Columns[sortData.Index];
        var order = sortData.Order;

        if (column.PropertyName == null)
        {
            throw new InvalidOperationException($"Please set the {nameof(column.PropertyName)} of the column");
        }

        if (!column.IsSortable(this))
        {
            throw new InvalidOperationException($"{column.PropertyName} column is not sortable");
        }

        if (!this.IsSortable)
        {
            throw new InvalidOperationException("DataGrid is not sortable");
        }

        var items = this.InternalItems;

        switch (order)
        {
            case SortingOrder.Ascendant:
                items = items.OrderBy(x => ReflectionUtils.GetValueByPath(x, column.PropertyName)).ToList();
                _ = column.SortingIcon.RotateTo(0);
                break;
            case SortingOrder.Descendant:
                items = items.OrderByDescending(x => ReflectionUtils.GetValueByPath(x, column.PropertyName)).ToList();
                _ = column.SortingIcon.RotateTo(180);
                break;
        }

        for (var i = 0; i < this.Columns.Count; i++)
        {
            if (i != sortData.Index)
            {
                this._sortingOrders[i] = SortingOrder.None;
                this.Columns[i].SortingIconContainer.IsVisible = false;
            }
            else
            {
                this.Columns[i].SortingIconContainer.IsVisible = true;
            }
        }

        this._internalItems = items;

        this._sortingOrders[sortData.Index] = order;
        this.SortedColumnIndex = sortData;

        this._collectionView.ItemsSource = this._internalItems;
    }

    #endregion Sorting methods

    #region Methods

    /// <summary>
    /// Scrolls to the row
    /// </summary>
    /// <param name="item">Item to scroll</param>
    /// <param name="position">Position of the row in screen</param>
    /// <param name="animated">animated</param>
    public void ScrollTo(object item, ScrollToPosition position, bool animated = true) => this._collectionView.ScrollTo(item, position: position, animate: animated);

    #endregion Methods

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
        BindableProperty.Create(nameof(Columns), typeof(List<DataGridColumn>), typeof(DataGrid),
            propertyChanged: (b, _, _) => ((DataGrid)b).InitHeaderView(),
            defaultValueCreator: _ => new List<DataGridColumn>());

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

                if (self.SelectedItem != null && self.InternalItems?.Contains(self.SelectedItem) != true)
                {
                    self.SelectedItem = null;
                }
            });

    private void HandleItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is IEnumerable items)
        {
            this.InternalItems = new List<object>(items.Cast<object>());
            if (this.SelectedItem != null && !this.InternalItems.Contains(this.SelectedItem))
            {
                this.SelectedItem = null;
            }
        }
    }

    public static readonly BindableProperty RowHeightProperty =
        BindableProperty.Create(nameof(RowHeight), typeof(int), typeof(DataGrid), 40);

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
                if (v is null)
                {
                    return null;
                }

                var self = (DataGrid)b;

                if (!self.SelectionEnabled)
                {
                    throw new InvalidOperationException("Datagrid must be SelectionEnabled=true to set SelectedItem");
                }

                if (self.InternalItems?.Contains(v) == true)
                {
                    return v;
                }

                return null;
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

    public static readonly BindableProperty RefreshingEnabledProperty =
    BindableProperty.Create(nameof(RefreshingEnabled), typeof(bool), typeof(DataGrid), true,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (n is bool refreshingEnabled)
                {
                    _ = self.PullToRefreshCommand?.CanExecute(() => refreshingEnabled);
                }
            });

    public static readonly BindableProperty PullToRefreshCommandProperty =
        BindableProperty.Create(nameof(PullToRefreshCommand), typeof(ICommand), typeof(DataGrid), null,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
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
                    (sData.Index < self.Columns.Count && self.Columns[sData.Index].SortingEnabled);
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

    public static readonly BindableProperty SortIconProperty =
        BindableProperty.Create(nameof(SortIcon), typeof(Polygon), typeof(DataGrid));

    public static readonly BindableProperty SortIconStyleProperty =
        BindableProperty.Create(nameof(SortIconStyle), typeof(Style), typeof(DataGrid), null,
            propertyChanged: (b, _, n) =>
            {
                if (b is DataGrid self && n is Style style)
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
                if (o != n)
                {
                    ((DataGrid)b)._collectionView.EmptyView = n as View;
                }
            });

    #endregion Bindable properties

    #region Properties

    /// <summary>
    /// Selected Row color
    /// </summary>
    public Color ActiveRowColor
    {
        get => (Color)this.GetValue(ActiveRowColorProperty);
        set => this.SetValue(ActiveRowColorProperty, value);
    }

    /// <summary>
    /// BackgroundColor of the column header
    /// Default value is White
    /// </summary>
    public Color HeaderBackground
    {
        get => (Color)this.GetValue(HeaderBackgroundProperty);
        set => this.SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>
    /// Border color
    /// Default Value is Black
    /// </summary>
    public Color BorderColor
    {
        get => (Color)this.GetValue(BorderColorProperty);
        set => this.SetValue(BorderColorProperty, value);
    }

    /// <summary>
    /// Background color of the rows. It repeats colors consecutively for rows.
    /// </summary>
    public IColorProvider RowsBackgroundColorPalette
    {
        get => (IColorProvider)this.GetValue(RowsBackgroundColorPaletteProperty);
        set => this.SetValue(RowsBackgroundColorPaletteProperty, value);
    }

    /// <summary>
    /// Text color of the rows. It repeats colors consecutively for rows.
    /// </summary>
    public IColorProvider RowsTextColorPalette
    {
        get => (IColorProvider)this.GetValue(RowsTextColorPaletteProperty);
        set => this.SetValue(RowsTextColorPaletteProperty, value);
    }

    /// <summary>
    /// ItemsSource of the DataGrid
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)this.GetValue(ItemsSourceProperty);
        set => this.SetValue(ItemsSourceProperty, value);
    }

    private IList<object>? _internalItems;

    internal IList<object>? InternalItems
    {
        get => this._internalItems;
        set
        {
            this._internalItems = value;

            if (this.IsSortable && this.SortedColumnIndex != null)
            {
                this.SortItems(this.SortedColumnIndex);
            }
            else
            {
                this._collectionView.ItemsSource = this._internalItems;
            }
        }
    }

    /// <summary>
    /// Columns
    /// </summary>
    public List<DataGridColumn> Columns
    {
        get => (List<DataGridColumn>)this.GetValue(ColumnsProperty);
        set => this.SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Font size of the cells.
    /// It does not sets header font size. Use <c>HeaderLabelStyle</c> to set header font size.
    /// </summary>
    public double FontSize
    {
        get => (double)this.GetValue(FontSizeProperty);
        set => this.SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Sets the font family.
    /// It does not sets header font family. Use <c>HeaderLabelStyle</c> to set header font size.
    /// </summary>
    public string FontFamily
    {
        get => (string)this.GetValue(FontFamilyProperty);
        set => this.SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Sets the row height
    /// </summary>
    public int RowHeight
    {
        get => (int)this.GetValue(RowHeightProperty);
        set => this.SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Sets header height
    /// </summary>
    public int HeaderHeight
    {
        get => (int)this.GetValue(HeaderHeightProperty);
        set => this.SetValue(HeaderHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets if the grid is sortable. Default value is true.
    /// Sortable columns must implement <see cref="IComparable"/>
    /// If you want to enable or disable sorting for specific column please use <c>SortingEnabled</c> property
    /// </summary>
    public bool IsSortable
    {
        get => (bool)this.GetValue(IsSortableProperty);
        set => this.SetValue(IsSortableProperty, value);
    }

    /// <summary>
    /// Enables selection in dataGrid. Default value is True
    /// </summary>
    public bool SelectionEnabled
    {
        get => (bool)this.GetValue(SelectionEnabledProperty);
        set => this.SetValue(SelectionEnabledProperty, value);
    }

    /// <summary>
    /// Selected item
    /// </summary>
    public object? SelectedItem
    {
        get => this.GetValue(SelectedItemProperty);
        set => this.SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Executes the command when refreshing via pull
    /// </summary>
    public ICommand PullToRefreshCommand
    {
        get => (ICommand)this.GetValue(PullToRefreshCommandProperty);
        set => this.SetValue(PullToRefreshCommandProperty, value);
    }

    /// <summary>
    /// Displays an ActivityIndicator when is refreshing
    /// </summary>
    public bool IsRefreshing
    {
        get => (bool)this.GetValue(IsRefreshingProperty);
        set => this.SetValue(IsRefreshingProperty, value);
    }

    /// <summary>
    /// Enables refreshing the DataGrid by a pull down command
    /// </summary>
    public bool RefreshingEnabled
    {
        get => (bool)this.GetValue(RefreshingEnabledProperty);
        set => this.SetValue(RefreshingEnabledProperty, value);
    }

    /// <summary>
    /// Border thickness for header &amp; each cell
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)this.GetValue(BorderThicknessProperty);
        set => this.SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Determines to show the borders of header cells.
    /// Default value is <c>true</c>
    /// </summary>
    public bool HeaderBordersVisible
    {
        get => (bool)this.GetValue(HeaderBordersVisibleProperty);
        set => this.SetValue(HeaderBordersVisibleProperty, value);
    }

    /// <summary>
    /// Column index and sorting order for the DataGrid
    /// </summary>
    public SortData SortedColumnIndex
    {
        get => (SortData)this.GetValue(SortedColumnIndexProperty);
        set => this.SetValue(SortedColumnIndexProperty, value);
    }

    /// <summary>
    /// Style of the header label.
    /// Style's <c>TargetType</c> must be Label.
    /// </summary>
    public Style HeaderLabelStyle
    {
        get => (Style)this.GetValue(HeaderLabelStyleProperty);
        set => this.SetValue(HeaderLabelStyleProperty, value);
    }

    /// <summary>
    /// Sort icon
    /// </summary>
    public Polygon SortIcon
    {
        get => (Polygon)this.GetValue(SortIconProperty);
        set => this.SetValue(SortIconProperty, value);
    }

    /// <summary>
    /// Style of the sort icon
    /// Style's <c>TargetType</c> must be Polygon.
    /// </summary>
    public Style SortIconStyle
    {
        get => (Style)this.GetValue(SortIconStyleProperty);
        set => this.SetValue(SortIconStyleProperty, value);
    }

    /// <summary>
    /// View to show when there is no data to display
    /// </summary>
    public View NoDataView
    {
        get => (View)this.GetValue(NoDataViewProperty);
        set => this.SetValue(NoDataViewProperty, value);
    }

    #endregion Properties

    #region UI Methods

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (this.SelectionEnabled)
        {
            if (this.Parent is null)
            {
                this._collectionView.SelectionChanged -= this.OnSelectionChanged;
            }
            else
            {
                this._collectionView.SelectionChanged += this.OnSelectionChanged;
            }
        }

        if (this.RefreshingEnabled)
        {
            if (this.Parent is null)
            {
                this._refreshView.Refreshing -= this.OnRefreshing;
            }
            else
            {
                this._refreshView.Refreshing += this.OnRefreshing;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        this.InitHeaderView();
    }

    private void OnRefreshing(object? sender, EventArgs e) => this._refreshingEventManager.HandleEvent(this, e, nameof(Refreshing));

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        this.SelectedItem = this._collectionView.SelectedItem;

        this._itemSelectedEventManager.HandleEvent(this, e, nameof(ItemSelected));
    }

    internal void Reload()
    {
        if (this._internalItems is not null)
        {
            this.InternalItems = new List<object>(this._internalItems);
        }
        this.RefreshHeaderColumnWidths();
    }

    private void RefreshHeaderColumnWidths()
    {
        for (var i = 0; i < this.Columns.Count; i++)
        {
            var column = this.Columns[i];

            this._headerView.ColumnDefinitions[i] = column.ColumnDefinition;
        }
    }

    #endregion UI Methods

    #region Header Creation Methods

    private View GetHeaderViewForColumn(DataGridColumn column, int index)
    {
        column.HeaderLabel.Style = column.HeaderLabelStyle ??
                                   this.HeaderLabelStyle ?? (Style)this._headerView.Resources["HeaderDefaultStyle"];

        if (this.IsSortable && column.IsSortable(this) && column.SortingEnabled)
        {
            column.SortingIcon.Style = this.SortIconStyle ?? (Style)this._headerView.Resources["SortIconStyle"];
            column.SortingIconContainer.HeightRequest = this.HeaderHeight * 0.3;
            column.SortingIconContainer.WidthRequest = this.HeaderHeight * 0.3;

            var grid = new Grid
            {
                ColumnSpacing = 0,
                Padding = new(0, 0, 4, 0),
                ColumnDefinitions = new()
                {
                    new() { Width = new(1, GridUnitType.Star) },
                    new() { Width = new(1, GridUnitType.Auto) }
                },
                Children = { column.HeaderLabel, column.SortingIconContainer },
                GestureRecognizers =
                {
                    new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            var order = this._sortingOrders[index] == SortingOrder.Ascendant
                                ? SortingOrder.Descendant
                                : SortingOrder.Ascendant;

                            this.SortedColumnIndex = new(index, order);
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
        this.SetColumnsBindingContext();

        this._headerView.Children.Clear();
        this._headerView.ColumnDefinitions.Clear();
        this._sortingOrders.Clear();

        this._headerView.Padding = new(this.BorderThickness.Left, this.BorderThickness.Top, this.BorderThickness.Right, 0);
        this._headerView.ColumnSpacing = this.BorderThickness.HorizontalThickness;

        if (this.Columns != null)
        {
            for (var i = 0; i < this.Columns.Count; i++)
            {
                var col = this.Columns[i];

                col.ColumnDefinition ??= new(col.Width);

                this._headerView.ColumnDefinitions.Add(col.ColumnDefinition);

                var cell = this.GetHeaderViewForColumn(col, i);

                cell.SetBinding(BackgroundColorProperty, new Binding(nameof(this.HeaderBackground), source: this));

                cell.SetBinding(IsVisibleProperty,
                    new Binding(nameof(col.IsVisible), BindingMode.OneWay, source: col));

                Grid.SetColumn(cell, i);
                this._headerView.Children.Add(cell);

                this._sortingOrders.Add(i, SortingOrder.None);
            }
        }
    }

    private void SetColumnsBindingContext()
    {
        if (this.Columns != null)
        {
            foreach (var c in this.Columns)
            {
                c.BindingContext = this.BindingContext;
            }
        }
    }

    #endregion Header Creation Methods
}
