/*-----------------------------------------------------------------------
The MIT License (MIT)
-------------------------------------------------------------------------
Copyright (c) 2017 James Montemagno (https://github.com/jamesmontemagno/mvvm-helpers)
Copyright (c) 2016 .NET Foundation Contributors (https://github.com/xamarin/XamarinCommunityToolkit)
-------------------------------------------------------------------------
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
-----------------------------------------------------------------------*/
namespace Maui.DataGrid.Collections;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The object type of elements in the collection.</typeparam>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via collection expression")]
internal sealed class ObservableRangeCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableRangeCollection{T}"/> class.
    /// </summary>
    public ObservableRangeCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableRangeCollection{T}"/> class that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">collection: The collection from which the elements are copied.</param>
    /// <exception cref="ArgumentNullException">The collection parameter cannot be null.</exception>
    public ObservableRangeCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T).
    /// </summary>
    /// <param name="collection">The collection to add.</param>
    /// <param name="notificationMode">The notification mode.</param>
    /// <exception cref="ArgumentException">An exception when an invalid notification mode is set.</exception>
    public void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
    {
        if (notificationMode is not NotifyCollectionChangedAction.Add and not NotifyCollectionChangedAction.Reset)
        {
            throw new ArgumentException("Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
        }

        ArgumentNullException.ThrowIfNull(collection);

        CheckReentrancy();

        var startIndex = Count;

        var itemsAdded = AddArrangeCore(collection);

        if (!itemsAdded)
        {
            return;
        }

        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);

            return;
        }

        var changedItems = collection is List<T> list ? list : [.. collection];

        RaiseChangeNotificationEvents(
            action: NotifyCollectionChangedAction.Add,
            changedItems: changedItems,
            startingIndex: startIndex);
    }

    /// <summary>
    /// Removes the first occurrence of each item in the specified collection from ObservableCollection(Of T). NOTE: with notificationMode = Remove, removed items starting index is not set because items are not guaranteed to be consecutive.
    /// </summary>
    /// <param name="collection">The collection to remove.</param>
    /// <param name="notificationMode">The notification mode.</param>
    /// <exception cref="ArgumentException">An exception when an invalid notification mode is set.</exception>
    public void RemoveRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Reset)
    {
        if (notificationMode is not NotifyCollectionChangedAction.Remove and not NotifyCollectionChangedAction.Reset)
        {
            throw new ArgumentException("Mode must be either Remove or Reset for RemoveRange.", nameof(notificationMode));
        }

        ArgumentNullException.ThrowIfNull(collection);

        CheckReentrancy();

        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            var raiseEvents = false;
            foreach (var item in collection)
            {
                if (Items.Remove(item))
                {
                    raiseEvents = true;
                }
            }

            if (raiseEvents)
            {
                RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);
            }

            return;
        }

        var changedItems = new List<T>(collection);
        for (var i = 0; i < changedItems.Count; i++)
        {
            if (!Items.Remove(changedItems[i]))
            {
                changedItems.RemoveAt(i); // Can't use a foreach because changedItems is intended to be (carefully) modified
                i--;
            }
        }

        if (changedItems.Count == 0)
        {
            return;
        }

        RaiseChangeNotificationEvents(
            action: NotifyCollectionChangedAction.Remove,
            changedItems: changedItems);
    }

    /// <summary>
    /// Clears the current collection and replaces it with the specified item.
    /// </summary>
    /// <param name="item">The item to replace the collection with.</param>
    public void Replace(T item) => ReplaceRange([item]);

    /// <summary>
    /// Clears the current collection and replaces it with the specified collection.
    /// </summary>
    /// <param name="collection">The collection to replace with.</param>
    public void ReplaceRange(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        CheckReentrancy();

        var previouslyEmpty = Items.Count == 0;

        Items.Clear();

        _ = AddArrangeCore(collection);

        var currentlyEmpty = Items.Count == 0;

        if (previouslyEmpty && currentlyEmpty)
        {
            return;
        }

        RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);
    }

    private bool AddArrangeCore(IEnumerable<T> collection)
    {
        var itemAdded = false;
        foreach (var item in collection)
        {
            Items.Add(item);
            itemAdded = true;
        }

        return itemAdded;
    }

    private void RaiseChangeNotificationEvents(NotifyCollectionChangedAction action, List<T>? changedItems = null, int startingIndex = -1)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

        if (changedItems is null)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action));
        }
        else
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems: changedItems, startingIndex: startingIndex));
        }
    }
}
