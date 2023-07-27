# Maui.DataGrid

DataGrid library for .NET **MAUI** applications.

[![NuGet](https://img.shields.io/badge/nuget-v3.0.0-blue.svg?style=plastic)](https://www.nuget.org/packages/akgul.Maui.Datagrid) [![CodeQL](https://github.com/akgulebubekir/Maui.DataGrid/actions/workflows/codeql.yml/badge.svg)](https://github.com/akgulebubekir/Maui.DataGrid/actions/workflows/codeql.yml)

> **Supported Platforms**
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
                <DataTemplate>
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
                <DataTemplate>
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

Screenshots
----------

![Screenshots](https://raw.githubusercontent.com/akgulebubekir/Maui.DataGrid/master/Screenshots/windows_landscape.PNG)


## Repository Activity

![Alt](https://repobeats.axiom.co/api/embed/850b3036e03f7eff1bb74b4744e42aa3901a8ee7.svg "Repobeats analytics")
