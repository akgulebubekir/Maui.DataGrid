namespace Maui.DataGrid;

using System.ComponentModel;

/// <summary>
/// Creates SortData for DataGrid
/// </summary>
[TypeConverter(typeof(SortDataTypeConverter))]
public sealed class SortData
{
    /// <summary>
    /// Implicitly converts an integer to a SortData object.
    /// </summary>
    /// <param name="index">The column index.</param>
    /// <returns>A SortData object.</returns>
    public static implicit operator SortData(int index) => FromInt32(index);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is SortData other)
        {
            return other.Index == Index && other.Order == Order;
        }

        return false;
    }

    #region ctor

    public SortData()
    { }

    public SortData(int index, SortingOrder order)
    {
        Index = index;
        Order = order;
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

    public static SortData FromInt32(int index) => new()
    {
        Index = Math.Abs(index),
        Order = index < 0 ? SortingOrder.Descendant : SortingOrder.Ascendant
    };

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Index, Order);
}
