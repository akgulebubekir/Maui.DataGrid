namespace Maui.DataGrid.Tests;

using System.Collections.ObjectModel;
using Xunit;

/// <summary>
/// Regression tests for issue #233: a header title long enough to be truncated could not be read in full.
/// Setting <c>ToolTipProperties.Text</c> on the column did nothing, because a
/// <see cref="DataGridColumn"/> is a plain <see cref="BindableObject"/> which never enters the visual
/// tree — the attached property had no element to attach to. The column now carries its own
/// <see cref="DataGridColumn.HeaderToolTip"/>, which it puts on the header element it actually owns.
/// </summary>
public class HeaderToolTipTest
{
    [Fact]
    public void Issue233_TheTitleIsOfferedAsTheToolTipWhenNoneIsSet()
    {
        var column = new DataGridColumn { Title = "A title long enough to be truncated" };

        // Whether a title is truncated is not knowable until layout, so it is always offered.
        Assert.Equal("A title long enough to be truncated", GetToolTip(column));
    }

    [Fact]
    public void Issue233_AnExplicitToolTipWins()
    {
        var column = new DataGridColumn { Title = "Won", HeaderToolTip = "Games won at home" };

        Assert.Equal("Games won at home", GetToolTip(column));
    }

    [Fact]
    public void Issue233_TheToolTipFollowsTheTitleChanging()
    {
        var column = new DataGridColumn { Title = "Won" };

        column.Title = "Lost";

        Assert.Equal("Lost", GetToolTip(column));
    }

    [Fact]
    public void Issue233_AnExplicitToolTipIsUnaffectedByTheTitleChanging()
    {
        var column = new DataGridColumn { Title = "Won", HeaderToolTip = "Games won at home" };

        column.Title = "Lost";

        Assert.Equal("Games won at home", GetToolTip(column));
    }

    [Fact]
    public void Issue233_TheToolTipFollowsTheToolTipChanging()
    {
        var column = new DataGridColumn { Title = "Won", HeaderToolTip = "Games won at home" };

        column.HeaderToolTip = "Games won away";

        Assert.Equal("Games won away", GetToolTip(column));
    }

    [Fact]
    public void Issue233_ClearingTheToolTipFallsBackToTheTitle()
    {
        var column = new DataGridColumn { Title = "Won", HeaderToolTip = "Games won at home" };

        column.HeaderToolTip = null;

        Assert.Equal("Won", GetToolTip(column));
    }

    [Fact]
    public void Issue233_AnEmptyToolTipAsksForNone()
    {
        // Unset means "use the title"; empty means "no tooltip", which is the only way to opt out of a
        // title a consumer does not want repeated.
        var column = new DataGridColumn { Title = "Won", HeaderToolTip = string.Empty };

        Assert.Null(GetToolTip(column));
    }

    [Fact]
    public void Issue233_AFormattedTitleIsOfferedWhenThereIsNoPlainTitle()
    {
        var column = new DataGridColumn
        {
            FormattedTitle = new FormattedString
            {
                Spans =
                {
                    new Span { Text = "Home" },
                    new Span { Text = " (won-lost)" },
                },
            },
        };

        // A formatted title has no Title to fall back on, and is the more likely of the two to be long.
        Assert.Equal("Home (won-lost)", GetToolTip(column));
    }

    [Fact]
    public void Issue233_AColumnWithNoTitleAtAllHasNoToolTip()
    {
        var column = new DataGridColumn { PropertyName = "Name" };

        Assert.Null(GetToolTip(column));
    }

    [Fact]
    public void Issue233_TheToolTipIsCarriedByAnElementInTheHeaderRow()
    {
        var dataGrid = new DataGrid
        {
            ItemsSource = new ObservableCollection<TestItem> { new() { Name = "First" } },
            Columns = [new DataGridColumn { Title = "Name", PropertyName = nameof(TestItem.Name) }],
        };

        dataGrid.HeaderRow.InitializeHeaderRow(force: true);

        // This is the whole defect: the tooltip has to live on something which is really in the tree,
        // which the column itself never is.
        var headerCell = dataGrid.Columns[0].HeaderCell;

        Assert.NotNull(headerCell);
        Assert.Contains(dataGrid.Columns[0].HeaderLabelContainer, Descendants(headerCell));
        Assert.Contains(headerCell, dataGrid.HeaderRow.Children);
    }

    private static string? GetToolTip(DataGridColumn column) =>
        (string?)ToolTipProperties.GetText(column.HeaderLabelContainer);

    private static IEnumerable<Element> Descendants(Element element)
    {
        foreach (var child in ((IVisualTreeElement)element).GetVisualChildren())
        {
            if (child is Element childElement)
            {
                yield return childElement;

                foreach (var descendant in Descendants(childElement))
                {
                    yield return descendant;
                }
            }
        }
    }

    private sealed class TestItem
    {
        public required string Name { get; init; }
    }
}
