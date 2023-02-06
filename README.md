# Maui.DataGrid

DataGrid library for .NET **MAUI** applications.

[![NuGet](https://img.shields.io/badge/nuget-v2.0.0-blue.svg?style=plastic)](https://www.nuget.org/packages/akgul.Maui.Datagrid) [![CodeQL](https://github.com/akgulebubekir/Maui.DataGrid/actions/workflows/codeql.yml/badge.svg)](https://github.com/akgulebubekir/Maui.DataGrid/actions/workflows/codeql.yml)

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
             HeaderBackground="{StaticResource GridHeaderBgColor}"
             PullToRefreshCommand="{Binding RefreshCommand}" IsRefreshing="{Binding IsRefreshing}"
             ActiveRowColor="{StaticResource ActiveRowColor}">
    <dg:DataGrid.Columns>
        <dg:DataGridColumn Title="Logo" PropertyName="Logo" Width="150" SortingEnabled="False">
            <dg:DataGridColumn.CellTemplate>
                <DataTemplate>
                    <Image Source="{Binding}" HorizontalOptions="Center" VerticalOptions="Center"
                           Aspect="AspectFit" HeightRequest="60" />
                </DataTemplate>
            </dg:DataGridColumn.CellTemplate>
        </dg:DataGridColumn>
        <dg:DataGridColumn Title="Team" PropertyName="Name" />
        <dg:DataGridColumn Title="Won" PropertyName="Won" />
        <dg:DataGridColumn Title="Lost" PropertyName="Lost" />
        <dg:DataGridColumn PropertyName="Home">
            <dg:DataGridColumn.FormattedTitle>
                <FormattedString>
                    <Span Text="Home" TextColor="Black" FontSize="13" FontAttributes="Bold" />
                    <Span Text=" (win-loose)" TextColor="#333333" FontSize="11" />
                </FormattedString>
            </dg:DataGridColumn.FormattedTitle>
        </dg:DataGridColumn>
        <dg:DataGridColumn Title="Percentage" PropertyName="Percentage" StringFormat="{}{0:0.00}" />
        <dg:DataGridColumn Title="Streak" PropertyName="Streak" Width="0.5*">
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
    <dg:DataGrid.Resources>
        <ResourceDictionary>
            <conv:StreakToColorConverter x:Key="StreakToColorConverter" />
        </ResourceDictionary>
    </dg:DataGrid.Resources>
</dg:DataGrid>
```

Screenshots
----------

![Screenshots](https://raw.githubusercontent.com/akgulebubekir/Maui.DataGrid/master/Screenshots/windows_landscape.PNG)
