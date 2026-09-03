# Maui.DataGrid

DataGrid library for .NET **MAUI** applications.

[![NuGet version (akgul.Maui.Datagrid)](https://img.shields.io/nuget/v/akgul.Maui.Datagrid.svg)](https://www.nuget.org/packages/akgul.Maui.Datagrid)
[![CodeQL](https://github.com/akgulebubekir/Maui.DataGrid/actions/workflows/codeql.yml/badge.svg)](https://github.com/akgulebubekir/Maui.DataGrid/actions/workflows/codeql.yml)

- [Supported Platforms](#supported-platforms)
- [Requirements](#requirements)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Features](#features)
- [API Reference](#api-reference)
- [Obsolete Members](#obsolete-members)
- [Dependencies](#dependencies)
- [Building From Source](#building-from-source)
- [Tip](#tip)
- [Contributing](#contributing)
- [License](#license)

## Supported Platforms

The library itself targets `net10.0` and contains no platform-specific code, so it runs anywhere
.NET MAUI runs. The minimum OS versions below are the ones declared by the sample app in this
repository, and are the versions the library is exercised against.

| Platform | Minimum version | Status |
| --- | --- | --- |
| Android | API 24 (Android 7.0) | Built and tested by the sample app |
| iOS | 16.0 | Built and tested by the sample app |
| MacCatalyst | 15.0 | Built and tested by the sample app |
| Windows | 10.0.19041.0 (targeting `10.0.26100.0`) | Built and tested by the sample app |
| Tizen | 6.5 | Should work; the sample's Tizen target is commented out and not built in CI |
| Other MAUI platforms | — | Expected to work, not verified |

Windows is only added to the sample's target frameworks when the build host is Windows, so the
sample can be restored and built on macOS and Linux without the Windows SDK.

To build the sample for Tizen, uncomment the Tizen target framework in
[Maui.DataGrid.Sample.csproj](Maui.DataGrid.Sample/Maui.DataGrid.Sample.csproj) and install the
Tizen tooling from [Tizen.NET](https://github.com/Samsung/Tizen.NET).

## Requirements

To **consume** the NuGet package:

- A .NET MAUI app on .NET 10 (`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`,
  `net10.0-windows...`, etc.)
- .NET MAUI 10.0.80 or newer (`Microsoft.Maui.Controls`)

The package references `Microsoft.Maui.Controls` with `PrivateAssets="all"`, so it does not force a
MAUI version on your app — your app's own `MauiVersion` is used. No `MauiProgram` registration or
`Use...()` call is needed; only the XAML namespace (see [Getting Started](#getting-started)).

To **build this repository**:

- .NET SDK **10.0.301** or newer — pinned in [global.json](global.json) with
  `"rollForward": "latestFeature"`
- The .NET MAUI workload: `dotnet workload restore`
- Platform SDKs for whichever targets you build (Android SDK, Xcode for iOS/MacCatalyst, Windows SDK
  for Windows)
- Optional: Visual Studio 2022 (latest, with the ".NET Multi-platform App UI development" workload)
  or VS Code with the .NET MAUI extension

`RestorePackagesWithLockFile` is enabled and `RestoreLockedMode` is turned on for CI builds, so
`packages.lock.json` must be committed whenever a package reference changes. The library also builds
with `IsTrimmable` and `IsAotCompatible`, and static analysis is strict
(`AnalysisLevel=latest-all`, `EnforceCodeStyleInBuild`, StyleCop) — warnings will fail your local
build if you introduce them.

## Installation

```shell
dotnet add package akgul.Maui.DataGrid
```

Or via the Package Manager console:

```powershell
Install-Package akgul.Maui.DataGrid
```

## Getting Started

Add the XAML namespace and declare a `DataGrid` with its columns:

```xaml
 xmlns:dg="clr-namespace:Maui.DataGrid;assembly=Maui.DataGrid"

<dg:DataGrid ItemsSource="{Binding Teams}" SelectionMode="Single" SelectedItem="{Binding SelectedTeam}"
                RowHeight="70" HeaderHeight="50" BorderColor="{StaticResource GridBorderColor}"
                HeaderBackground="{StaticResource GridHeaderBgColor}" HeaderBordersVisible="{Binding HeaderBordersVisible}"
                PullToRefreshCommand="{Binding RefreshCommand}" IsRefreshing="{Binding IsRefreshing}" PaginationEnabled="{Binding PaginationEnabled}" PageSize="5"
                ActiveRowColor="{StaticResource ActiveRowColor}">
    <dg:DataGrid.Columns>
        <dg:DataGridColumn Title="Logo" PropertyName="Logo" SortingEnabled="False">
            <dg:DataGridColumn.CellTemplate>
                <DataTemplate x:DataType="x:String">
                    <Image Source="{Binding}" HorizontalOptions="Center" VerticalOptions="Center"
                           Aspect="AspectFit" HeightRequest="60" />
                </DataTemplate>
            </dg:DataGridColumn.CellTemplate>
        </dg:DataGridColumn>
        <dg:DataGridColumn Title="Team" PropertyName="Name" IsVisible="{Binding TeamColumnVisible}" Width="{Binding TeamColumnWidth}" />
        <dg:DataGridColumn Title="Won" PropertyName="Won" Width="0.5*" IsVisible="{Binding WonColumnVisible}" />
        <dg:DataGridColumn Title="Lost" PropertyName="Lost" Width="0.5*" />
        <dg:DataGridColumn PropertyName="Home">
            <dg:DataGridColumn.FormattedTitle>
                <FormattedString>
                    <Span Text="Home" TextColor="Black" FontSize="13" FontAttributes="Bold" />
                    <Span Text=" (won-lost)" TextColor="#333333" FontSize="11" />
                </FormattedString>
            </dg:DataGridColumn.FormattedTitle>
        </dg:DataGridColumn>
        <dg:DataGridColumn Title="Win %" PropertyName="Percentage" Width="0.75*" StringFormat="{}{0:0.00}" />
        <dg:DataGridColumn Title="Streak" PropertyName="Streak" Width="0.75*">
            <dg:DataGridColumn.CellTemplate>
                <DataTemplate x:DataType="m:Streak">
                    <ContentView HorizontalOptions="Fill" VerticalOptions="Fill"
                                 BackgroundColor="{Binding Converter={StaticResource StreakToColorConverter}}">
                        <Label Text="{Binding}" HorizontalOptions="Center" VerticalOptions="Center"
                               TextColor="Black" />
                    </ContentView>
                </DataTemplate>
            </dg:DataGridColumn.CellTemplate>
        </dg:DataGridColumn>
    </dg:DataGrid.Columns>
    <dg:DataGrid.RowsBackgroundColorPalette>
        <dg:PaletteCollection>
            <Color>#F2F2F2</Color>
            <Color>#FFFFFF</Color>
        </dg:PaletteCollection>
    </dg:DataGrid.RowsBackgroundColorPalette>
</dg:DataGrid>
```

A complete, runnable example lives in [Maui.DataGrid.Sample](Maui.DataGrid.Sample/) — see
[MainPage.xaml](Maui.DataGrid.Sample/MainPage.xaml).

## Features

### Columns and cells

`PropertyName` supports nested property paths, resolved by reflection against the runtime type of
each intermediate value:

```xaml
<dg:DataGridColumn Title="City" PropertyName="Address.City" />
```

`Width` accepts the same units as `Grid`: absolute (`120`), star (`0.5*`), or `Auto`. An `Auto` column is
sized to the widest of its header cell and the cells of the rows currently on screen, and the header and
every row are given that one width. Only the realized rows are measured, so scrolling to a longer value
widens the column at that point rather than in advance; use an absolute width where that shift is
unwelcome.

Use `StringFormat` for simple formatting, or `CellTemplate` for arbitrary content. Without a
`CellTemplate`, a cell renders as a `Label` bound to `PropertyName`.

`CellTemplate` and `EditCellTemplate` also accept a `DataTemplateSelector`, which is resolved per
row — `SelectTemplate` receives the row's item — so a cell's content can vary with its data.

Cells are created once per on-screen row and reused as rows are recycled while scrolling, so cell
content should get everything it displays from its bindings rather than from work done when the
template is instantiated. A `DataTemplateSelector` is re-consulted whenever a row is recycled, and
its cell is rebuilt only if the selector picks a different template for the new item.

### Sorting

Sorting is enabled by default (`DataGrid.SortingEnabled`), and each column can opt out with
`DataGridColumn.SortingEnabled="False"`. A column's underlying type must implement `IComparable` to
be sortable; `DataGridColumn.IsSortable()` reports whether it does.

`SortedColumnIndex` is a `SortData` (index + `SortingOrder`) and is two-way bindable. An `int`
implicitly converts to `SortData`, where a negative index means a descending sort:

```xaml
<!-- Sort ascending on column 1 -->
<dg:DataGrid SortedColumnIndex="1" />

<!-- Sort descending on column 1 -->
<dg:DataGrid SortedColumnIndex="-1" />
```

### Filtering

Set `DataGrid.FilteringEnabled="True"` to show a filter `Entry` in each header cell. Individual
columns can opt out with `DataGridColumn.FilteringEnabled="False"`, and `DataGridColumn.FilterText`
is bindable so filters can be driven or read from a view model. Changing a filter resets
`PageNumber` to 1.

### Pagination

Set `PaginationEnabled="True"` to show the pagination footer. `PageSize` defaults to `100`, must be
greater than zero, and the page-size picker offers `5, 10, 50, 100, 200, 1000` unless you supply your
own `PageSizeList`. `PageCount` is read-only (`OneWayToSource`). `PageText` and `PerPageText` exist
so the footer labels can be localized, and `PageSizeVisible="False"` hides the page-size picker.

### Selection

`SelectionMode` (`None`, `Single`, `Multiple`) replaces the obsolete `SelectionEnabled`. Use
`SelectedItem` for `Single` and `SelectedItems` for `Multiple` — switching modes clears the one that
no longer applies. Both are coerced against the grid's current items, so a selection that is not
present in `ItemsSource` is dropped.

`ItemSelected` (event) fires on selection change. `RowTappedCommand` does too by default, receiving
the `SelectionChangedEventArgs` — which means it does *not* fire when the already-selected row is
tapped again, nor at all while `SelectionMode="None"`. Set `RowTappedCommandMode="Tap"` to have every
row tap execute the command with the **tapped item** as its parameter instead, regardless of
`SelectionMode`:

```xaml
<dg:DataGrid RowTappedCommand="{Binding RowTapped}" RowTappedCommandMode="Tap" />
```

In `Tap` mode the command is not executed from the selection-change path, so a tap executes it
exactly once. `RowTappedCommandMode` defaults to `SelectionChanged` for backwards compatibility; the
default is expected to change in the next major version.

### Editing

Bind `RowToEdit` to the item that should render in edit mode. Cells in that row use
`DataGridColumn.EditCellTemplate` (default: an `Entry` bound to `PropertyName`) instead of
`CellTemplate`.

### Pull to refresh

Bind `PullToRefreshCommand` (optionally with `PullToRefreshCommandParameter`) and `IsRefreshing`.
`RefreshingEnabled` toggles the gesture, `RefreshColor` sets the spinner color, and the `Refreshing`
event is raised when a refresh starts.

### Row colors

`RowsBackgroundColorPalette` and `RowsTextColorPalette` take any `IColorProvider`. `PaletteCollection`
is the built-in implementation and cycles its colors across rows. Implement `IColorProvider` yourself
for data-driven colors:

```csharp
internal sealed class OverdueColorProvider : IColorProvider
{
    public Color GetColor(int rowIndex, object item) =>
        item is Invoice { IsOverdue: true } ? Colors.MistyRose : Colors.White;
}
```

`ActiveRowColor` is the color of the selected row.

`GetColor` is re-evaluated for every visible row whenever the displayed items change — adding,
removing, sorting, filtering, or changing page — so a row's color always matches its current index.

### Empty state

`NoDataView` is shown when the grid has no rows (it maps to the underlying `CollectionView`'s
`EmptyView`).

### Scrolling

```csharp
dataGrid.ScrollTo(item, ScrollToPosition.MakeVisible, animated: true);
```

### Threading

An `ItemsSource` collection may be added to, removed from, or cleared on any thread — the grid
marshals the resulting sort, filter, and pagination work to the UI thread itself, so a background
worker filling a collection needs no `MainThread.BeginInvokeOnMainThread` of its own. Note that this
covers mutations of the collection only: the grid's properties, `ItemsSource` included, must be set on
the UI thread like those of any other MAUI control.

### Styling

`HeaderLabelStyle`, `HeaderFilterStyle`, `SortIconStyle`, and `PaginationStepperStyle` override the
grid's defaults; the first three can also be set per column. Setting one of them back to `null`
restores the built-in default. `BorderColor`, `BorderThickness`, `HeaderBordersVisible`,
`HeaderBackground`, `FooterBackground`, `FooterTextColor`, `FontFamily`, and `FontSize` cover the
rest of the chrome.

Borders are not drawn as outlines: each cell is inset by half of `BorderThickness` over a surface
painted in `BorderColor`, and the surface showing through those insets is what looks like a grid
line. So `BorderColor` is the grid line colour, and `BorderThickness="0"` removes the surface along
with the lines — set it to zero (or `HeaderBordersVisible="False"` for the header alone) to see the
grid's own background through the rows.

## API Reference

### `DataGrid`

All of the following are bindable properties.

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `ItemsSource` | `IEnumerable` | `null` | Rows to display. `INotifyCollectionChanged` sources are observed for changes. |
| `Columns` | `ObservableCollection<DataGridColumn>` | empty | Column definitions. |
| `SelectionMode` | `SelectionMode` | `Single` | `None`, `Single`, or `Multiple`. Two-way. |
| `SelectedItem` | `object?` | `null` | Selected row in `Single` mode. Two-way. |
| `SelectedItems` | `IList<object>` | empty | Selected rows in `Multiple` mode. Two-way. |
| `RowTappedCommand` | `ICommand` | `null` | Executed on row tap. Parameter and trigger depend on `RowTappedCommandMode`. |
| `RowTappedCommandMode` | `RowTappedCommandMode` | `SelectionChanged` | `SelectionChanged` passes `SelectionChangedEventArgs` on selection change; `Tap` passes the tapped item on every tap. |
| `RowToEdit` | `object` | `null` | Row rendered using `EditCellTemplate`. |
| `SortingEnabled` | `bool` | `true` | Enables sorting for the grid. |
| `SortedColumnIndex` | `SortData?` | `null` | Current sort. Two-way. Negative `int` means descending. |
| `SortIcon` | `Polygon` | `null` | Custom sort indicator shape. |
| `SortIconStyle` | `Style` | built-in | Style for the sort indicator. |
| `FilteringEnabled` | `bool` | `false` | Shows per-column filter inputs. |
| `PaginationEnabled` | `bool` | `false` | Shows the pagination footer. |
| `PageNumber` | `int` | `1` | Current page. Two-way. |
| `PageCount` | `int` | `1` | Total pages. `OneWayToSource`. |
| `PageSize` | `int` | `100` | Rows per page; must be `> 0`. Two-way. |
| `PageSizeList` | `IList<int>` | `5, 10, 50, 100, 200, 1000` | Choices in the page-size picker. |
| `PageSizeVisible` | `bool` | `true` | Shows the page-size picker. |
| `PageText` | `string` | `"Page:"` | Localizable page label. |
| `PerPageText` | `string` | `"# per page:"` | Localizable per-page label. |
| `PaginationStepperStyle` | `Style?` | built-in | Style for the pagination stepper. |
| `RefreshingEnabled` | `bool` | `true` | Enables pull-to-refresh. |
| `PullToRefreshCommand` | `ICommand` | `null` | Executed on pull-to-refresh. |
| `PullToRefreshCommandParameter` | `object` | `null` | Parameter for the refresh command. |
| `IsRefreshing` | `bool` | `false` | Refresh indicator state. Two-way. |
| `RefreshColor` | `Color` | `Purple` | Refresh spinner color. |
| `RowHeight` | `int` | `40` | Row height. |
| `HeaderHeight` | `int` | `40` | Header height. |
| `FooterHeight` | `int` | `50` on Android, `40` elsewhere | Footer height. |
| `HeaderBackground` | `Color` | `White` | Header background. |
| `HeaderBordersVisible` | `bool` | `true` | Draws borders in the header. |
| `HeaderLabelStyle` | `Style` | built-in | Style for header labels (`TargetType` must be `Label`). |
| `HeaderFilterStyle` | `Style` | built-in | Style for header filter inputs. |
| `FooterBackground` | `Color` | `White` | Footer background. |
| `FooterTextColor` | `Color` | `Black` | Footer text color. |
| `BorderColor` | `Color` | `Black` | Grid line color, and the color of the surface the cells sit on. |
| `BorderThickness` | `Thickness` | `1` | Grid border thickness. Zero on every edge leaves no lines and a transparent surface. Two-way. |
| `ActiveRowColor` | `Color` | `RGB(128, 144, 160)` | Selected row color. |
| `RowsBackgroundColorPalette` | `IColorProvider` | `PaletteCollection { White }` | Per-row background colors. |
| `RowsTextColorPalette` | `IColorProvider` | `PaletteCollection { Black }` | Per-row text colors. |
| `FontFamily` | `string` | `Font.Default.Family` | Cell font family. |
| `FontSize` | `double` | `13.0` | Cell font size. |
| `ItemSizingStrategy` | `ItemSizingStrategy` | `MeasureFirstItem` | Sizing strategy of the underlying `CollectionView`. |
| `NoDataView` | `View` | `null` | Shown when there are no rows. |

**Events**

| Event | Signature | Description |
| --- | --- | --- |
| `ItemSelected` | `EventHandler<SelectionChangedEventArgs>` | Raised when the selection changes. |
| `Refreshing` | `EventHandler` | Raised when a pull-to-refresh starts. |

**Methods**

| Method | Description |
| --- | --- |
| `ScrollTo(object item, ScrollToPosition position, bool animated = true)` | Scrolls the given row into view. |

### `DataGridColumn`

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `PropertyName` | `string` | `null` | Property path to bind, e.g. `Name` or `Address.City`. |
| `Title` | `string` | `""` | Header text. |
| `FormattedTitle` | `FormattedString` | `null` | Rich header text; overrides `Title`. |
| `Width` | `GridLength` | `Star` | Column width (absolute, star, or auto). |
| `IsVisible` | `bool` | `true` | Shows or hides the column. |
| `StringFormat` | `string?` | `null` | Format string for the default cell. |
| `CellTemplate` | `DataTemplate?` | `null` | Display template; defaults to a `Label`. |
| `EditCellTemplate` | `DataTemplate?` | `null` | Edit template; defaults to an `Entry`. |
| `SortingEnabled` | `bool` | `true` | Allows sorting on this column. |
| `FilteringEnabled` | `bool` | `true` | Allows filtering on this column. |
| `FilterText` | `string` | `null` | Current filter value. Two-way bindable. |
| `LineBreakMode` | `LineBreakMode` | `WordWrap` | Text wrapping for the default cell. |
| `HorizontalContentAlignment` | `LayoutOptions` | `Center` | Horizontal cell alignment. |
| `VerticalContentAlignment` | `LayoutOptions` | `Center` | Vertical cell alignment. |
| `Padding` | `Thickness` | `0` | Cell padding. |
| `HeaderLabelStyle` | `Style` | inherited | Header label style for this column. |
| `HeaderFilterStyle` | `Style` | inherited | Header filter style for this column. |

**Events**

| Event | Signature | Description |
| --- | --- | --- |
| `SizeChanged` | `EventHandler` | Raised when `Width` changes. |
| `VisibilityChanged` | `EventHandler` | Raised when `IsVisible` changes. |

**Methods**

| Method | Description |
| --- | --- |
| `IsSortable()` | Returns whether the column's resolved data type implements `IComparable`. |

### Supporting types

| Type | Description |
| --- | --- |
| `SortData` | `Index` + `Order` pair describing the current sort. Converts implicitly from `int`; `SortData.FromInt32(int)` treats a negative index as descending. Value-equality via `Equals`/`GetHashCode`. |
| `SortingOrder` | `None`, `Ascendant`, `Descendant`. |
| `IColorProvider` | `Color GetColor(int rowIndex, object item)` — implement to color rows from data. |
| `PaletteCollection` | `List<Color>` implementing `IColorProvider`; cycles colors across rows. Falls back to `White` when empty. |

## Obsolete Members

| Obsolete | Use instead |
| --- | --- |
| `DataGrid.IsSortable` / `IsSortableProperty` | `DataGrid.SortingEnabled` / `SortingEnabledProperty` |
| `DataGrid.SelectionEnabled` / `SelectionEnabledProperty` | `DataGrid.SelectionMode` / `SelectionModeProperty` |

## Dependencies

Current package version: **4.0.6**.

| Dependency | Version | Where |
| --- | --- | --- |
| .NET SDK | `10.0.301` (`rollForward: latestFeature`) | [global.json](global.json) |
| `Microsoft.Maui.Controls` | `10.0.80` (`$(MauiVersion)`) | [Directory.Build.props](Directory.Build.props) |
| Library target framework | `net10.0` | [Maui.DataGrid.csproj](Maui.DataGrid/Maui.DataGrid.csproj) |
| `DotNet.ReproducibleBuilds` | `2.0.5` (build-only) | [Directory.Build.props](Directory.Build.props) |
| `StyleCop.Analyzers` | `1.2.0-beta.556` (build-only) | [Directory.Build.props](Directory.Build.props) |
| `CommunityToolkit.Maui` | `14.2.0` | sample only |
| `xunit` | `2.9.3` | tests only |
| `xunit.runner.visualstudio` | `3.0.0` | tests only |
| `Microsoft.NET.Test.Sdk` | `17.12.0` | tests only |
| `coverlet.collector` | `6.0.2` | tests only |

Only `Microsoft.Maui.Controls` matters to consumers, and it is referenced with `PrivateAssets="all"`
and `ExcludeAssets="runtime"`, so the shipped package adds no runtime dependencies of its own beyond
MAUI itself.

## Building From Source

```shell
git clone https://github.com/akgulebubekir/Maui.DataGrid.git
cd Maui.DataGrid
dotnet workload restore
dotnet build Maui.DataGrid/Maui.DataGrid.csproj
```

Run the tests:

```shell
dotnet test Maui.DataGrid.Tests/Maui.DataGrid.Tests.csproj
```

Run the sample app (Windows builds unpackaged, so `dotnet run` works directly):

```shell
dotnet run --project Maui.DataGrid.Sample -f net10.0-windows10.0.26100.0
```

For other platforms pick the matching target framework, for example:

```shell
dotnet build Maui.DataGrid.Sample -t:Run -f net10.0-android
dotnet build Maui.DataGrid.Sample -t:Run -f net10.0-maccatalyst
```

The solution file is [Maui.DataGrid.slnx](Maui.DataGrid.slnx).

## Tip

If you are experiencing any issues on iOS, you can try adding the following to MauiProgram.cs

```csharp
#if IOS || MACCATALYST
builder.ConfigureMauiHandlers(handlers =>
{
    handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
});
#endif
```

## Screenshots

![Screenshot 2025-01-10 144417](https://github.com/user-attachments/assets/0f8b3bb1-a4e9-4620-bef2-c6821150fe12)

## Contributing

Issues and pull requests are welcome. Before opening a PR:

- Build with the pinned SDK; the repo uses strict analysis (`AnalysisLevel=latest-all`,
  `EnforceCodeStyleInBuild`, StyleCop, `WarningLevel=9999`) and treats
  [.editorconfig](.editorconfig) / [stylecop.json](stylecop.json) as the style source of truth.
- Run `dotnet test Maui.DataGrid.Tests/Maui.DataGrid.Tests.csproj`.
- Commit updated `packages.lock.json` files if you change any package reference — CI restores in
  locked mode and will fail otherwise.
- Public API changes are checked against the `PackageValidationBaselineVersion` in
  [Maui.DataGrid.csproj](Maui.DataGrid/Maui.DataGrid.csproj); breaking changes need a baseline bump
  or a suppression entry.

## License

Licensed under the [MIT License](LICENSE).

## Repository Activity

![Alt](https://repobeats.axiom.co/api/embed/850b3036e03f7eff1bb74b4744e42aa3901a8ee7.svg "Repobeats analytics")

## Star History

[![Star History Chart](https://star-history.dera.page/svg?repos=akgulebubekir/Maui.DataGrid&type=Date)](https://star-history.dera.page/#akgulebubekir/Maui.DataGrid&Date)
