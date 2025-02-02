# Maui.DataGrid

DataGrid library for .NET **MAUI** applications.

[![NuGet version (akgul.Maui.Datagrid)](https://img.shields.io/nuget/v/akgul.Maui.Datagrid.svg)](https://www.nuget.org/packages/akgul.Maui.Datagrid)
[![CodeQL](https://github.com/akgulebubekir/Maui.DataGrid/actions/workflows/codeql.yml/badge.svg)](https://github.com/akgulebubekir/Maui.DataGrid/actions/workflows/codeql.yml)

> **Supported Platforms**
  >
  >- Android
  >- iOS
  >- MacCatalyst
  >- Tizen
  >- Windows
  >- and any other platform that MAUI runs on

```xaml
 xmlns:dg="clr-namespace:Maui.DataGrid;assembly=Maui.DataGrid"

<dg:DataGrid ItemsSource="{Binding Teams}" SelectionEnabled="True" SelectedItem="{Binding SelectedTeam}"
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

## Repository Activity

![Alt](https://repobeats.axiom.co/api/embed/850b3036e03f7eff1bb74b4744e42aa3901a8ee7.svg "Repobeats analytics")

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=akgulebubekir/Maui.DataGrid&type=Date)](https://star-history.com/#akgulebubekir/Maui.DataGrid&Date)
