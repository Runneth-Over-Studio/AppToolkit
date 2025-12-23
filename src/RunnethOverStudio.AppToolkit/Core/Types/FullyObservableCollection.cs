using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace RunnethOverStudio.AppToolkit.Core.Types;

/// <summary>
/// An observable collection that also listens for property changes on its items and raises an event when an item's property changes.
/// </summary>
/// <remarks>
/// Inspired by <see href="https://stackoverflow.com/a/32013610">this answer</see> by Bob Sammers.
/// </remarks>
public class FullyObservableCollection<T> : ObservableCollectionWithSelection<T> where T : class, INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when a property is changed within an item.
    /// </summary>
    public event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullyObservableCollection{T}"/> class.
    /// </summary>
    public FullyObservableCollection() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FullyObservableCollection{T}"/> class that contains elements copied from the specified list.
    /// </summary>
    /// <param name="list">The list whose elements are copied to the new collection.</param>
    public FullyObservableCollection(List<T> list) : base(list)
    {
        ObserveAll();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FullyObservableCollection{T}"/> class that contains elements copied from the specified enumerable.
    /// </summary>
    /// <param name="enumerable">The collection whose elements are copied to the new collection.</param>
    public FullyObservableCollection(IEnumerable<T> enumerable) : base(enumerable)
    {
        ObserveAll();
    }

    /// <inheritdoc/>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace) && e.OldItems != null)
        {
            foreach (T item in e.OldItems)
            {
                item.PropertyChanged -= ChildPropertyChanged;
            }
        }

        if ((e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace) && e.NewItems != null)
        {
            foreach (T item in e.NewItems)
            {
                item.PropertyChanged += ChildPropertyChanged;
            }
        }

        base.OnCollectionChanged(e);
    }

    /// <summary>
    /// Raises the <see cref="ItemPropertyChanged"/> event with the specified event arguments.
    /// </summary>
    /// <param name="e">The event data containing information about the property change.</param>
    protected void OnItemPropertyChanged(ItemPropertyChangedEventArgs e)
    {
        ItemPropertyChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="ItemPropertyChanged"/> event for the item at the specified index.
    /// </summary>
    /// <param name="index">The index of the item whose property changed.</param>
    /// <param name="e">The event data containing information about the property change.</param>
    protected void OnItemPropertyChanged(int index, PropertyChangedEventArgs e)
    {
        OnItemPropertyChanged(new ItemPropertyChangedEventArgs(index, e));
    }

    /// <inheritdoc/>
    protected override void ClearItems()
    {
        foreach (T item in Items)
        {
            item.PropertyChanged -= ChildPropertyChanged;
        }

        base.ClearItems();
    }

    private void ObserveAll()
    {
        foreach (T item in Items)
        {
            item.PropertyChanged += ChildPropertyChanged;
        }
    }

    private void ChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender != null)
        {
            T typedSender = (T)sender;
            int i = Items.IndexOf(typedSender);

            if (i < 0)
            {
                throw new ArgumentException("Received property notification from item not in collection");
            }

            OnItemPropertyChanged(i, e);
        }
    }
}

/// <summary>
/// Provides data for the <see cref="FullyObservableCollection{T}.ItemPropertyChanged"/> event.
/// </summary>
public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
{
    /// <summary>
    /// Gets the index in the collection for which the property change has occurred.
    /// </summary>
    /// <value>
    /// Index in parent collection.
    /// </value>
    public int CollectionIndex { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
    /// </summary>
    /// <param name="index">The index in the collection of changed item.</param>
    /// <param name="name">The name of the property that changed.</param>
    public ItemPropertyChangedEventArgs(int index, string? name) : base(name)
    {
        CollectionIndex = index;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    public ItemPropertyChangedEventArgs(int index, PropertyChangedEventArgs args) : this(index, args.PropertyName) { }
}
