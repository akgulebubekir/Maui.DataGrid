namespace Maui.DataGrid;

using System.ComponentModel;

/// <summary>
/// Creates SortData for DataGrid
/// </summary>
[TypeConverter(typeof(SortDataTypeConverter))]
public sealed class SortData
{
    public static implicit operator SortData(int index) => new()
    {
        Index = Math.Abs(index),
        Order = index < 0 ? SortingOrder.Descendant : SortingOrder.Ascendant,
    };

    public override bool Equals(object? obj)
    {
        if (obj is SortData other)
        {
            return other.Index == this.Index && other.Order == this.Order;
        }

        return false;
    }

    #region ctor

    public SortData()
    { }

    public SortData(int index, SortingOrder order)
    {
        this.Index = index;
        this.Order = order;
    }

    #endregion ctor

    #region Properties

    /// <summary>
    /// Sorting order for the column
    /// </summary>
    public SortingOrder Order { get; set; }

    /// <summary>
    /// Column Index to sort
    /// </summary>
    public int Index { get; set; }

    #endregion Properties

    public override int GetHashCode() => throw new NotImplementedException();
}
