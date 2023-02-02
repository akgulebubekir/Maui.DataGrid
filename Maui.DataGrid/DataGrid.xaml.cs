namespace Maui.DataGrid;

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Maui.DataGrid.Utils;
using Microsoft.Maui.Controls.Shapes;
using Font = Microsoft.Maui.Font;

/// <summary>
/// DataGrid component for Maui.
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DataGrid
{
    /// <summary>
    /// BindableProperty that sets the color of the selected row.
    /// </summary>
    public static readonly BindableProperty ActiveRowColorProperty =
        BindableProperty.Create(nameof(ActiveRowColor), typeof(Color), typeof(DataGrid), Color.FromRgb(128, 144, 160));

    /// <summary>
    /// BindableProperty that sets the header background color.
    /// </summary>
    public static readonly BindableProperty HeaderBackgroundProperty =
        BindableProperty.Create(nameof(HeaderBackground), typeof(Color), typeof(DataGrid), Colors.White, propertyChanged: OnHeaderBackgroundChanged);

    /// <summary>
    /// BindableProperty that sets the border color around every cell.
    /// </summary>
    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(DataGrid), Colors.Black, propertyChanged: OnBorderColorChanged);

    /// <summary>
    /// BindableProperty that sets the alternating background color palette.
    /// </summary>
    public static readonly BindableProperty RowsBackgroundColorPaletteProperty =
        BindableProperty.Create(nameof(RowsBackgroundColorPalette), typeof(IColorProvider), typeof(DataGrid), new PaletteCollection { Colors.White }, propertyChanged: OnColorPaletteChanged);

    /// <summary>
    /// BindableProperty that sets the alternating text color palette.
    /// </summary>
    public static readonly BindableProperty RowsTextColorPaletteProperty =
        BindableProperty.Create(nameof(RowsTextColorPalette), typeof(IColorProvider), typeof(DataGrid), new PaletteCollection { Colors.Black }, propertyChanged: OnColorPaletteChanged);

    /// <summary>
    /// BindableProperty that sets the DataGridColumns for this DataGrid.
    /// </summary>
    public static readonly BindableProperty ColumnsProperty =
        BindableProperty.Create(nameof(Columns), typeof(List<DataGridColumn>), typeof(DataGrid), new List<DataGridColumn>(), propertyChanged: (b, _, _) => ((DataGrid)b).InitHeaderView());

    /// <summary>
    /// BindableProperty that sets the list of objects that forms the binding context of the DataGrid.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(DataGrid), null, propertyChanged: OnItemsSourceChanged);

    /// <summary>
    /// BindableProperty that sets the height of every normal row upon page load.
    /// </summary>
    public static readonly BindableProperty RowHeightProperty =
        BindableProperty.Create(nameof(RowHeight), typeof(int), typeof(DataGrid), 40);

    /// <summary>
    /// BindableProperty that sets the height of the header row upon page load.
    /// </summary>
    public static readonly BindableProperty HeaderHeightProperty =
        BindableProperty.Create(nameof(HeaderHeight), typeof(int), typeof(DataGrid), 40);

    /// <summary>
    /// BindableProperty that sets whether sorting is enabled for the DataGrid.
    /// </summary>
    public static readonly BindableProperty SortingEnabledProperty =
        BindableProperty.Create(nameof(SortingEnabled), typeof(bool), typeof(DataGrid), true);

    /// <summary>
    /// BindableProperty that sets the font size for the DataGrid.
    /// </summary>
    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(DataGrid), 13.0);

    /// <summary>
    /// BindableProperty that sets the font family for the DataGrid.
    /// </summary>
    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(DataGrid), Font.Default.Family);

    /// <summary>
    /// BindableProperty that gets and sets selected item for the DataGrid.
    /// </summary>
    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(DataGrid), null, BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged, coerceValue: SetSelectedItem);

    /// <summary>
    /// BindableProperty that sets whether selecting items is enabled for this DataGrid.
    /// </summary>
    public static readonly BindableProperty SelectionEnabledProperty =
        BindableProperty.Create(nameof(SelectionEnabled), typeof(bool), typeof(DataGrid), true, propertyChanged: OnSelectionEnabledChanged);

    /// <summary>
    /// BindableProperty that sets whether refreshing via the PullToRefreshCommandProperty is enabled for this DataGrid.
    /// </summary>
    public static readonly BindableProperty RefreshingEnabledProperty =
        BindableProperty.Create(nameof(RefreshingEnabled), typeof(bool), typeof(DataGrid), true, propertyChanged: OnRefreshingEnabledChanged);

    /// <summary>
    /// BindableProperty that sets the command used to refresh the DataGrid when doing a pull down gesture.
    /// </summary>
    public static readonly BindableProperty PullToRefreshCommandProperty =
        BindableProperty.Create(nameof(PullToRefreshCommand), typeof(ICommand), typeof(DataGrid), null, propertyChanged: OnPullToRefreshChanged);

    /// <summary>
    /// BindableProperty that gets or sets whether Refreshing is occurring.
    /// </summary>
    public static readonly BindableProperty IsRefreshingProperty =
        BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(DataGrid), false, BindingMode.TwoWay, propertyChanged: OnIsRefreshingChanged);

    /// <summary>
    /// BindableProperty that gets or sets the thickness of the borders around the cells.
    /// </summary>
    public static readonly BindableProperty BorderThicknessProperty =
        BindableProperty.Create(nameof(BorderThickness), typeof(Thickness), typeof(DataGrid), new Thickness(1), propertyChanged: OnBorderThicknessChanged);

    /// <summary>
    /// BindableProperty that gets or sets whether the header has borders.
    /// </summary>
    public static readonly BindableProperty HeaderBordersVisibleProperty =
        BindableProperty.Create(nameof(HeaderBordersVisible), typeof(bool), typeof(DataGrid), true, propertyChanged: OnHeaderBordersVisibleChanged);

    /// <summary>
    /// BindableProperty that gets or sets the index of the column that is sorted.
    /// </summary>
    public static readonly BindableProperty SortedColumnIndexProperty =
        BindableProperty.Create(
            nameof(SortedColumnIndex),
            typeof(SortData),
            typeof(DataGrid),
            null,
            BindingMode.TwoWay,
            validateValue: (b, v) =>
                   v is not SortData sData
                || b is not DataGrid self
                || self.Columns is null
                || self.Columns.Count == 0
                || (sData.Index < self.Columns.Count && self.Columns[sData.Index].SortingEnabled),
            propertyChanged: (b, o, n) =>
            {
                var self = (DataGrid)b;
                if (o != n)
                {
                    self.SortItems((SortData)n);
                }
            });

    /// <summary>
    /// BindableProperty that sets the styling of the header labels.
    /// </summary>
    public static readonly BindableProperty HeaderLabelStyleProperty =
        BindableProperty.Create(nameof(HeaderLabelStyle), typeof(Style), typeof(DataGrid));

    /// <summary>
    /// BindableProperty that gets or sets the polygon used for the sort icon.
    /// </summary>
    public static readonly BindableProperty SortIconProperty =
        BindableProperty.Create(nameof(SortIcon), typeof(Polygon), typeof(DataGrid));

    /// <summary>
    /// BindableProperty that gets or sets the styling of the sort icon.
    /// </summary>
    public static readonly BindableProperty SortIconStyleProperty =
        BindableProperty.Create(nameof(SortIconStyle), typeof(Style), typeof(DataGrid), null, propertyChanged: (b, _, n) =>
            {
                if (b is DataGrid self && n is Style style)
                {
                    foreach (var column in self.Columns)
                    {
                        column.SortingIcon.Style = style;
                    }
                }
            });

    /// <summary>
    /// BindableProperty that gets or sets the view used inside the grid when there is no data to display.
    /// </summary>
    public static readonly BindableProperty NoDataViewProperty =
        BindableProperty.Create(nameof(NoDataView), typeof(View), typeof(DataGrid), propertyChanged: (b, o, n) =>
            {
                if (o != n)
                {
                    ((DataGrid)b).collectionView.EmptyView = n as View;
                }
            });

    private readonly Dictionary<int, SortingOrder> sortingOrders;

    private readonly WeakEventManager itemSelectedEventManager = new();
    private readonly WeakEventManager refreshingEventManager = new();

    private IList<object>? internalItems;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGrid"/> class.
    /// </summary>
    public DataGrid()
    {
        this.InitializeComponent();

        this.sortingOrders = new();
    }

    /// <summary>
    /// Item selected event.
    /// </summary>
    public event EventHandler<SelectionChangedEventArgs> ItemSelected
    {
        add => this.itemSelectedEventManager.AddEventHandler(value);
        remove => this.itemSelectedEventManager.RemoveEventHandler(value);
    }

    /// <summary>
    /// Grid refreshing event.
    /// </summary>
    public event EventHandler Refreshing
    {
        add => this.refreshingEventManager.AddEventHandler(value);
        remove => this.refreshingEventManager.RemoveEventHandler(value);
    }

    /// <summary>
    /// Gets or sets selected Row color.
    /// </summary>
    public Color ActiveRowColor
    {
        get => (Color)this.GetValue(ActiveRowColorProperty);
        set => this.SetValue(ActiveRowColorProperty, value);
    }

    /// <summary>
    /// Gets or sets backgroundColor of the column header
    /// Default value is White.
    /// </summary>
    public Color HeaderBackground
    {
        get => (Color)this.GetValue(HeaderBackgroundProperty);
        set => this.SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets border color
    /// Default Value is Black.
    /// </summary>
    public Color BorderColor
    {
        get => (Color)this.GetValue(BorderColorProperty);
        set => this.SetValue(BorderColorProperty, value);
    }

    /// <summary>
    /// Gets or sets background color of the rows. It repeats colors consecutively for rows.
    /// </summary>
    public IColorProvider RowsBackgroundColorPalette
    {
        get => (IColorProvider)this.GetValue(RowsBackgroundColorPaletteProperty);
        set => this.SetValue(RowsBackgroundColorPaletteProperty, value);
    }

    /// <summary>
    /// Gets or sets text color of the rows. It repeats colors consecutively for rows.
    /// </summary>
    public IColorProvider RowsTextColorPalette
    {
        get => (IColorProvider)this.GetValue(RowsTextColorPaletteProperty);
        set => this.SetValue(RowsTextColorPaletteProperty, value);
    }

    /// <summary>
    /// Gets or sets itemsSource of the DataGrid.
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)this.GetValue(ItemsSourceProperty);
        set => this.SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets columns.
    /// </summary>
    public List<DataGridColumn> Columns
    {
        get => (List<DataGridColumn>)this.GetValue(ColumnsProperty);
        set => this.SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets font size of the cells.
    /// It does not sets header font size. Use <c>HeaderLabelStyle</c> to set header font size.
    /// </summary>
    public double FontSize
    {
        get => (double)this.GetValue(FontSizeProperty);
        set => this.SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the font family.
    /// It does not sets header font family. Use <c>HeaderLabelStyle</c> to set header font size.
    /// </summary>
    public string FontFamily
    {
        get => (string)this.GetValue(FontFamilyProperty);
        set => this.SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the row height.
    /// </summary>
    public int RowHeight
    {
        get => (int)this.GetValue(RowHeightProperty);
        set => this.SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets header height.
    /// </summary>
    public int HeaderHeight
    {
        get => (int)this.GetValue(HeaderHeightProperty);
        set => this.SetValue(HeaderHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets if the grid is sortable. Default value is true.
    /// Sortable columns must implement <see cref="IComparable"/>
    /// If you want to enable or disable sorting for specific column please use <c>SortingEnabled</c> property.
    /// </summary>
    public bool SortingEnabled
    {
        get => (bool)this.GetValue(SortingEnabledProperty);
        set => this.SetValue(SortingEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether enables selection in dataGrid. Default value is True.
    /// </summary>
    public bool SelectionEnabled
    {
        get => (bool)this.GetValue(SelectionEnabledProperty);
        set => this.SetValue(SelectionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets selected item.
    /// </summary>
    public object? SelectedItem
    {
        get => this.GetValue(SelectedItemProperty);
        set => this.SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets executes the command when refreshing via pull.
    /// </summary>
    public ICommand PullToRefreshCommand
    {
        get => (ICommand)this.GetValue(PullToRefreshCommandProperty);
        set => this.SetValue(PullToRefreshCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether displays an ActivityIndicator when is refreshing.
    /// </summary>
    public bool IsRefreshing
    {
        get => (bool)this.GetValue(IsRefreshingProperty);
        set => this.SetValue(IsRefreshingProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether enables refreshing the DataGrid by a pull down command.
    /// </summary>
    public bool RefreshingEnabled
    {
        get => (bool)this.GetValue(RefreshingEnabledProperty);
        set => this.SetValue(RefreshingEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets border thickness for header &amp; each cell.
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)this.GetValue(BorderThicknessProperty);
        set => this.SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether determines to show the borders of header cells.
    /// Default value is <c>true</c>.
    /// </summary>
    public bool HeaderBordersVisible
    {
        get => (bool)this.GetValue(HeaderBordersVisibleProperty);
        set => this.SetValue(HeaderBordersVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets column index and sorting order for the DataGrid.
    /// </summary>
    public SortData SortedColumnIndex
    {
        get => (SortData)this.GetValue(SortedColumnIndexProperty);
        set => this.SetValue(SortedColumnIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets style of the header label.
    /// Style's <c>TargetType</c> must be Label.
    /// </summary>
    public Style HeaderLabelStyle
    {
        get => (Style)this.GetValue(HeaderLabelStyleProperty);
        set => this.SetValue(HeaderLabelStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets sort icon.
    /// </summary>
    public Polygon SortIcon
    {
        get => (Polygon)this.GetValue(SortIconProperty);
        set => this.SetValue(SortIconProperty, value);
    }

    /// <summary>
    /// Gets or sets style of the sort icon
    /// Style's <c>TargetType</c> must be Polygon.
    /// </summary>
    public Style SortIconStyle
    {
        get => (Style)this.GetValue(SortIconStyleProperty);
        set => this.SetValue(SortIconStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets view to show when there is no data to display.
    /// </summary>
    public View NoDataView
    {
        get => (View)this.GetValue(NoDataViewProperty);
        set => this.SetValue(NoDataViewProperty, value);
    }

    /// <summary>
    /// Gets or sets the internal copy of ItemsSource.
    /// </summary>
    internal IList<object>? InternalItems
    {
        get => this.internalItems;
        set
        {
            this.internalItems = value;

            if (this.SortingEnabled && this.SortedColumnIndex != null)
            {
                this.SortItems(this.SortedColumnIndex);
            }
            else
            {
                this.collectionView.ItemsSource = this.internalItems;
            }
        }
    }

    /// <summary>
    /// Scrolls to the row.
    /// </summary>
    /// <param name="item">Item to scroll.</param>
    /// <param name="position">Position of the row in screen.</param>
    /// <param name="animated">animated.</param>
    public void ScrollTo(object item, ScrollToPosition position, bool animated = true) => this.collectionView.ScrollTo(item, position: position, animate: animated);

    /// <summary>
    /// Reloads all rows in the DataGrid, and refreshes all column widths.
    /// </summary>
    internal void Reload()
    {
        if (this.internalItems is not null)
        {
            this.InternalItems = new List<object>(this.internalItems);
        }

        this.RefreshHeaderColumnWidths();
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (this.SelectionEnabled)
        {
            if (this.Parent is null)
            {
                this.collectionView.SelectionChanged -= this.OnSelectionChanged;
            }
            else
            {
                this.collectionView.SelectionChanged += this.OnSelectionChanged;
            }
        }

        if (this.RefreshingEnabled)
        {
            if (this.Parent is null)
            {
                this.refreshView.Refreshing -= this.OnRefreshing;
            }
            else
            {
                this.refreshView.Refreshing += this.OnRefreshing;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        this.InitHeaderView();
    }

    private static void OnIsRefreshingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self && newValue is bool isRefreshing)
        {
            self.refreshView.IsRefreshing = isRefreshing;
        }
    }

    private static void OnBorderThicknessChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self && newValue is Thickness thickness)
        {
            self.headerView.ColumnSpacing = thickness.HorizontalThickness / 2;
            self.headerView.Padding = thickness.HorizontalThickness / 2;
        }
    }

    private static void OnHeaderBordersVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self && newValue is bool isVisible)
        {
            self.headerView.BackgroundColor = isVisible ? self.BorderColor : self.HeaderBackground;
        }
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self)
        {
            // ObservableCollection Tracking
            if (oldValue is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged -= self.OnItemsSourceCollectionChanged;
            }

            if (newValue == null)
            {
                self.InternalItems = null;
            }
            else
            {
                if (newValue is INotifyCollectionChanged changed)
                {
                    changed.CollectionChanged += self.OnItemsSourceCollectionChanged;
                }

                self.InternalItems = new List<object>(((IEnumerable)newValue).Cast<object>());
            }

            if (self.SelectedItem != null && self.InternalItems?.Contains(self.SelectedItem) != true)
            {
                self.SelectedItem = null;
            }
        }
    }

    private static void OnSelectionEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self && !self.SelectionEnabled && self.SelectedItem != null)
        {
            self.SelectedItem = null;
        }
    }

    private static void OnRefreshingEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self && newValue is bool refreshingEnabled)
        {
            _ = self.PullToRefreshCommand?.CanExecute(() => refreshingEnabled);
        }
    }

    private static void OnPullToRefreshChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self)
        {
            if (newValue is ICommand command)
            {
                self.refreshView.Command = command;
                _ = self.refreshView.Command?.CanExecute(self.RefreshingEnabled);
            }
            else
            {
                self.refreshView.Command = null;
            }
        }
    }

    private static void OnHeaderBackgroundChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self && self.headerView != null && !self.HeaderBordersVisible)
        {
            self.headerView.BackgroundColor = newValue as Color;
        }
    }

    private static void OnBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self)
        {
            if (self.HeaderBordersVisible)
            {
                self.headerView.BackgroundColor = newValue as Color;
            }

            if (self.Columns != null && self.ItemsSource != null)
            {
                self.Reload();
            }
        }
    }

    private static void OnColorPaletteChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self && self.Columns != null && self.ItemsSource != null)
        {
            self.Reload();
        }
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGrid self && self.collectionView.SelectedItem != newValue)
        {
            self.collectionView.SelectedItem = newValue;
        }
    }

    private static object? SetSelectedItem(BindableObject bindable, object value)
    {
        if (value is null)
        {
            return null;
        }

        if (bindable is DataGrid self)
        {
            if (!self.SelectionEnabled)
            {
                return null;
            }

            if (self.InternalItems?.Contains(value) == true)
            {
                return value;
            }
        }

        return null;
    }

    private void OnRefreshing(object? sender, EventArgs e) => this.refreshingEventManager.HandleEvent(this, e, nameof(this.Refreshing));

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        this.SelectedItem = this.collectionView.SelectedItem;

        this.itemSelectedEventManager.HandleEvent(this, e, nameof(this.ItemSelected));
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

    private void RefreshHeaderColumnWidths()
    {
        for (var i = 0; i < this.Columns.Count; i++)
        {
            var column = this.Columns[i];

            this.headerView.ColumnDefinitions[i] = column.ColumnDefinition;
        }
    }

    private View GetHeaderViewForColumn(DataGridColumn column, int index)
    {
        column.HeaderLabel.Style = column.HeaderLabelStyle ??
                                   this.HeaderLabelStyle ?? (Style)this.headerView.Resources["HeaderDefaultStyle"];

        if (this.SortingEnabled && column.IsSortable(this) && column.SortingEnabled)
        {
            column.SortingIcon.Style = this.SortIconStyle ?? (Style)this.headerView.Resources["SortIconStyle"];
            column.SortingIconContainer.HeightRequest = this.HeaderHeight * 0.3;
            column.SortingIconContainer.WidthRequest = this.HeaderHeight * 0.3;

            var grid = new Grid
            {
                ColumnSpacing = 0,
                Padding = new(0, 0, 4, 0),
                ColumnDefinitions = new()
                {
                    new() { Width = new(1, GridUnitType.Star) },
                    new() { Width = new(1, GridUnitType.Auto) },
                },
                Children = { column.HeaderLabel, column.SortingIconContainer },
                GestureRecognizers =
                {
                    new TapGestureRecognizer
                    {
                        Command = new Command(
                            () =>
                            {
                                var order = this.sortingOrders[index] == SortingOrder.Ascendant
                                    ? SortingOrder.Descendant
                                    : SortingOrder.Ascendant;

                                this.SortedColumnIndex = new(index, order);
                            },
                            () => column.SortingEnabled),
                    },
                },
            };

            Grid.SetColumn(column.SortingIconContainer, 1);
            return grid;
        }

        return new ContentView
        {
            Content = column.HeaderLabel,
        };
    }

    private void InitHeaderView()
    {
        this.SetColumnsBindingContext();

        this.headerView.Children.Clear();
        this.headerView.ColumnDefinitions.Clear();
        this.sortingOrders.Clear();

        this.headerView.Padding = new(this.BorderThickness.Left, this.BorderThickness.Top, this.BorderThickness.Right, 0);
        this.headerView.ColumnSpacing = this.BorderThickness.HorizontalThickness;

        if (this.Columns != null)
        {
            for (var i = 0; i < this.Columns.Count; i++)
            {
                var col = this.Columns[i];

                col.ColumnDefinition ??= new(col.Width);

                this.headerView.ColumnDefinitions.Add(col.ColumnDefinition);

                var cell = this.GetHeaderViewForColumn(col, i);

                cell.SetBinding(BackgroundColorProperty, new Binding(nameof(this.HeaderBackground), source: this));

                cell.SetBinding(IsVisibleProperty, new Binding(nameof(col.IsVisible), BindingMode.OneWay, source: col));

                Grid.SetColumn(cell, i);
                this.headerView.Children.Add(cell);

                this.sortingOrders.Add(i, SortingOrder.None);
            }
        }
    }

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

        if (!this.SortingEnabled)
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
                this.sortingOrders[i] = SortingOrder.None;
                this.Columns[i].SortingIconContainer.IsVisible = false;
            }
            else
            {
                this.Columns[i].SortingIconContainer.IsVisible = true;
            }
        }

        this.internalItems = items;

        this.sortingOrders[sortData.Index] = order;
        this.SortedColumnIndex = sortData;

        this.collectionView.ItemsSource = this.internalItems;
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
}
