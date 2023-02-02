namespace Maui.DataGrid;

using System.ComponentModel;

/// <summary>
/// Creates SortData for DataGrid.
/// </summary>
[TypeConverter(typeof(SortDataTypeConverter))]
public sealed class SortData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SortData"/> class.
    /// </summary>
    public SortData()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SortData"/> class.
    /// </summary>
    /// <param name="index">The column index to sort.</param>
    /// <param name="order">Ascending, descending, or none.</param>
    public SortData(int index, SortingOrder order)
    {
        this.Index = index;
        this.Order = order;
    }

    /// <summary>
    /// Gets or sets sorting order for the column.
    /// </summary>
    public SortingOrder Order { get; set; }

    /// <summary>
    /// Gets or sets column Index to sort.
    /// </summary>
    public int Index { get; set; }

    public static implicit operator SortData(int index) => new()
    {
        Index = Math.Abs(index),
        Order = index < 0 ? SortingOrder.Descendant : SortingOrder.Ascendant,
    };

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is SortData other)
        {
            return other.Index == this.Index && other.Order == this.Order;
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => throw new NotImplementedException();
}
