namespace Maui.DataGrid.Sample.Tests.TestUtils;

using System.ComponentModel;

/// <summary>
/// A simple ViewModel with a single property for testing bindings.
/// </summary>
/// <typeparam name="T">The type of the property.</typeparam>
internal sealed partial class SingleVM<T> : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public T? Item
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Item)));
        }
    }

    internal int NumberOfSubscribers => PropertyChanged?.GetInvocationList()?.Length ?? 0;
}
