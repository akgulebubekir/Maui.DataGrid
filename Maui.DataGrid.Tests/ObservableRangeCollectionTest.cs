namespace Maui.DataGrid.Tests;

using System.Collections.Specialized;
using Maui.DataGrid.Collections;
using Xunit;

public class ObservableRangeCollectionTest
{
    [Fact]
    public void AddRangeAddsItems()
    {
        var collection = new ObservableRangeCollection<int>();

        collection.AddRange([1, 2, 3]);

        Assert.Equal(3, collection.Count);
        Assert.Equal(1, collection[0]);
        Assert.Equal(2, collection[1]);
        Assert.Equal(3, collection[2]);
    }

    [Fact]
    public void AddRangeRaisesAddNotification()
    {
        var collection = new ObservableRangeCollection<int> { 10 };
        NotifyCollectionChangedEventArgs? args = null;

        collection.CollectionChanged += (s, e) => args = e;
        collection.AddRange([20, 30], NotifyCollectionChangedAction.Add);

        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Add, args!.Action);
        Assert.Equal([20, 30], args.NewItems!.Cast<int>());
        Assert.Equal(1, args.NewStartingIndex);
    }

    [Fact]
    public void AddRangeRaisesResetNotification()
    {
        var collection = new ObservableRangeCollection<int>();
        NotifyCollectionChangedEventArgs? args = null;

        collection.CollectionChanged += (s, e) => args = e;
        collection.AddRange([1, 2], NotifyCollectionChangedAction.Reset);

        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Reset, args.Action);
    }

    [Fact]
    public void AddRangeEmptyCollectionDoesNotRaiseEvent()
    {
        var collection = new ObservableRangeCollection<int>();
        var eventRaised = false;

        collection.CollectionChanged += (s, e) => eventRaised = true;
        collection.AddRange([]);

        Assert.False(eventRaised);
    }

    [Fact]
    public void AddRangeThrowsForInvalidMode()
    {
        var collection = new ObservableRangeCollection<int>();

        var ex = Assert.Throws<ArgumentException>(() =>
            collection.AddRange([1], NotifyCollectionChangedAction.Remove));
        Assert.Contains("Mode must be either Add or Reset", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddRangeThrowsForNullCollection()
    {
        var collection = new ObservableRangeCollection<int>();

        Assert.Throws<ArgumentNullException>(() =>
            collection.AddRange(null!));
    }

    [Fact]
    public void RemoveRangeRemovesItems()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2, 3, 4 };

        collection.RemoveRange([2, 4]);

        Assert.Equal(2, collection.Count);
        Assert.Contains(1, collection);
        Assert.Contains(3, collection);
    }

    [Fact]
    public void RemoveRangeRaisesResetNotification()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2, 3 };
        NotifyCollectionChangedEventArgs? args = null;

        collection.CollectionChanged += (s, e) => args = e;
        collection.RemoveRange([2], NotifyCollectionChangedAction.Reset);

        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Reset, args.Action);
    }

    [Fact]
    public void RemoveRangeRaisesRemoveNotification()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2, 3 };
        NotifyCollectionChangedEventArgs? args = null;

        collection.CollectionChanged += (s, e) => args = e;
        collection.RemoveRange([2, 3], NotifyCollectionChangedAction.Remove);

        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Remove, args!.Action);
        Assert.Equal([2, 3], args.OldItems!.Cast<int>());
    }

    [Fact]
    public void RemoveRangeWithRemoveModeExcludesNonExistingItems()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2, 3 };
        NotifyCollectionChangedEventArgs? args = null;

        collection.CollectionChanged += (s, e) => args = e;
        collection.RemoveRange([2, 99], NotifyCollectionChangedAction.Remove);

        Assert.NotNull(args);
        Assert.Equal([2], args!.OldItems!.Cast<int>());
    }

    [Fact]
    public void RemoveRangeNoMatchesDoesNotRaiseEvent()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2, 3 };
        var eventRaised = false;

        collection.CollectionChanged += (s, e) => eventRaised = true;
        collection.RemoveRange([99, 100]);

        Assert.False(eventRaised);
    }

    [Fact]
    public void RemoveRangeThrowsForInvalidMode()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2 };

        Assert.Throws<ArgumentException>(() =>
            collection.RemoveRange([1], NotifyCollectionChangedAction.Add));
    }

    [Fact]
    public void RemoveRangeThrowsForNullCollection()
    {
        var collection = new ObservableRangeCollection<int>();

        Assert.Throws<ArgumentNullException>(() =>
            collection.RemoveRange(null!));
    }

    [Fact]
    public void ReplaceReplacesAllWithSingleItem()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2, 3 };

        collection.Replace(42);

        Assert.Single(collection);
        Assert.Equal(42, collection[0]);
    }

    [Fact]
    public void ReplaceRangeReplacesAllItems()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2, 3 };
        NotifyCollectionChangedEventArgs? args = null;

        collection.CollectionChanged += (s, e) => args = e;
        collection.ReplaceRange([10, 20]);

        Assert.Equal(2, collection.Count);
        Assert.Equal(10, collection[0]);
        Assert.Equal(20, collection[1]);
        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Reset, args.Action);
    }

    [Fact]
    public void ReplaceRangeEmptyToEmptyDoesNotRaiseEvent()
    {
        var collection = new ObservableRangeCollection<int>();
        var eventRaised = false;

        collection.CollectionChanged += (s, e) => eventRaised = true;
        collection.ReplaceRange([]);

        Assert.False(eventRaised);
    }

    [Fact]
    public void ReplaceRangeNonEmptyToEmptyRaisesEvent()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2 };
        var eventRaised = false;

        collection.CollectionChanged += (s, e) => eventRaised = true;
        collection.ReplaceRange([]);

        Assert.True(eventRaised);
        Assert.Empty(collection);
    }

    [Fact]
    public void ReplaceRangeThrowsForNullCollection()
    {
        var collection = new ObservableRangeCollection<int>();

        Assert.Throws<ArgumentNullException>(() =>
            collection.ReplaceRange(null!));
    }

    [Fact]
    public void ConstructorWithCollectionCopiesItems()
    {
        var source = new[] { 10, 20, 30 };

        var collection = new ObservableRangeCollection<int>(source);

        Assert.Equal(3, collection.Count);
        Assert.Equal(10, collection[0]);
        Assert.Equal(20, collection[1]);
        Assert.Equal(30, collection[2]);
    }

    [Fact]
    public void AddRangeRaisesPropertyChangedForCount()
    {
        var collection = new ObservableRangeCollection<int>();
        var propertyNames = new List<string>();

        ((System.ComponentModel.INotifyPropertyChanged)collection).PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName!);
        collection.AddRange([1, 2]);

        Assert.Contains("Count", propertyNames);
        Assert.Contains("Item[]", propertyNames);
    }

    [Fact]
    public void RemoveRangeEmptyCollectionDoesNotRaiseEvent()
    {
        var collection = new ObservableRangeCollection<int>();
        var eventRaised = false;

        collection.CollectionChanged += (s, e) => eventRaised = true;
        collection.RemoveRange([]);

        Assert.False(eventRaised);
    }

    [Fact]
    public void ReplaceRangeRaisesPropertyChangedForCount()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2 };
        var propertyNames = new List<string>();

        ((System.ComponentModel.INotifyPropertyChanged)collection).PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName!);
        collection.ReplaceRange([10, 20, 30]);

        Assert.Contains("Count", propertyNames);
        Assert.Contains("Item[]", propertyNames);
    }

    [Fact]
    public void AddRangeAppendsToExistingItems()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2 };

        collection.AddRange([3, 4]);

        Assert.Equal(4, collection.Count);
        Assert.Equal(1, collection[0]);
        Assert.Equal(4, collection[3]);
    }
}
