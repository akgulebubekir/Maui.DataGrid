namespace Maui.DataGrid.Sample.Models;

internal sealed class Streak : IComparable
{
    public GameResult Result { get; set; }

    public int NumStreak { get; set; }

    public static bool operator ==(Streak left, Streak right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Streak left, Streak right)
    {
        return !(left == right);
    }

    public static bool operator <(Streak left, Streak right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(Streak left, Streak right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(Streak left, Streak right)
    {
        return left?.CompareTo(right) > 0;
    }

    public static bool operator >=(Streak left, Streak right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }

    public int CompareTo(object? obj)
    {
        if (obj is Streak s)
        {
            // First compare the Result
            var resultComparison = Result.CompareTo(s.Result);
            if (resultComparison != 0)
            {
                return resultComparison;
            }

            var winLossIndicator = Result == GameResult.Won ? 1 : -1;

            // If Result is the same, then compare the NumStreak
            return winLossIndicator * NumStreak.CompareTo(s.NumStreak);
        }

        throw new ArgumentException("Object is not a Streak");
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Enum.GetName(Result)} {NumStreak}";
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null)
        {
            return false;
        }

        return (Streak)obj == this;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Result, NumStreak);
    }
}
