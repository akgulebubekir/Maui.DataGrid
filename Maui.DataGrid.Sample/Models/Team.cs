namespace Maui.DataGrid.Sample.Models;

public class Team
{
    public string Name { get; set; }
    public int Win { get; set; }
    public int Loose { get; set; }
    public double Percentage { get; set; }
    public string Conf { get; set; }
    public string Div { get; set; }
    public string Home { get; set; }
    public string Road { get; set; }
    public string Last10 { get; set; }
    public Streak Streak { get; set; }
    public string Logo { get; set; }
}

public class Streak : IComparable
{
    public Result Result { get; set; }
    public int NumStreak { get; set; }

    public int CompareTo(object other)
    {
        var score = this.Result == Result.Win ? this.NumStreak : -this.NumStreak;
        if (other is Streak s)
        {
            var otherScore = s.Result == Result.Win ? s.NumStreak : -s.NumStreak;
            return score - otherScore;
        }

        return score;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Enum.GetName(typeof(Result), this.Result)} {this.NumStreak}";
}

public enum Result
{
    Loose = 0,
    Win
}
