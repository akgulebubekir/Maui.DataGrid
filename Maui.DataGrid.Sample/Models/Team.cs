namespace Maui.DataGrid.Sample.Models;

internal sealed class Team
{
    public required string Name { get; set; }

    public int Won { get; set; }

    public int Lost { get; set; }

    public double Percentage { get; set; }

    public required string Conf { get; set; }

    public required string Div { get; set; }

    public required string Home { get; set; }

    public required string Road { get; set; }

    public required string Last10 { get; set; }

    public required Streak Streak { get; set; }

    public required string Logo { get; set; }
}
