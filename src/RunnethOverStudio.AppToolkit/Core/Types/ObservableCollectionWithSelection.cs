using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace RunnethOverStudio.AppToolkit.Core.Types;

/// <summary>
/// Represents an observable collection that maintains a selected item and provides change notifications.
/// </summary>
/// <typeparam name="T">The type of elements in the collection. Must be a reference type.</typeparam>
/// <remarks>
/// Made available by Carl Franklin <see href="https://github.com/carlfranklin/AvnObservable">here</see>.
/// </remarks>
public class ObservableCollectionWithSelection<T> : ObservableCollection<T> where T : class
{
    /// <summary>
    /// Occurs before the selected item changes.
    /// </summary>
    public event EventHandler<T>? SelectedItemChanging;

    /// <summary>
    /// Occurs after the selected item has changed.
    /// </summary>
    public event EventHandler<T>? SelectedItemChanged;

    private int _selectedItemHashCode = 0;
    bool _updating = false;  // prevent reentrancy
    private T? _selectedItem;

    /// <summary>
    /// Gets or sets the currently selected item from the collection.
    /// </summary>
    /// <value>
    /// The selected item, or the first item in the collection if no item is explicitly selected and the collection is not empty.
    /// </value>
    /// <remarks>
    /// Setting this property raises the <see cref="SelectedItemChanging"/> and <see cref="SelectedItemChanged"/> events.
    /// If the value is <see langword="null"/>, the setter returns without making changes.
    /// </remarks>
    public T? SelectedItem
    {
        get
        {
            if (_selectedItem == null && this.Count > 0)
            {
                SelectedItem = this[0];
            }
            return _selectedItem;
        }
        set
        {
            if (value == null) return;
            if (_selectedItem != value)
            {
                UpdateSelectedItemInCollection();
                
                _updating = true; // prevent reentrancy
                _selectedItem = value;
                _selectedItemHashCode = value.GetHashCode();

                // Notify the UI.
                var args = new PropertyChangedEventArgs(nameof(SelectedItem));
                OnPropertyChanged(args);
                OnRaiseSelectedItemChangedEvent(value);
                
                _updating = false; // All done.
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCollectionWithSelection{T}"/> class.
    /// </summary>
    public ObservableCollectionWithSelection() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCollectionWithSelection{T}"/> class that contains elements copied from the specified list.
    /// </summary>
    /// <param name="list">The list whose elements are copied to the new collection.</param>
    public ObservableCollectionWithSelection(List<T> list) : base(list) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCollectionWithSelection{T}"/> class that contains elements copied from the specified enumerable.
    /// </summary>
    /// <param name="enumerable">The collection whose elements are copied to the new collection.</param>
    public ObservableCollectionWithSelection(IEnumerable<T> enumerable) : base(enumerable) { }

    /// <summary>
    /// Updates the selected item in the collection by replacing it with itself to trigger change notifications.
    /// </summary>
    /// <remarks>
    /// This method uses the hash code of the selected item to locate it in the collection and replace it,
    /// which ensures that data bindings and UI elements are notified of changes to the item's properties.
    /// Reentrancy is prevented during the update.
    /// </remarks>
    public void UpdateSelectedItemInCollection()
    {
        if (_updating || _selectedItem == null)
        {
            return;
        }

        _updating = true; // prevent reentrancy

        if (_selectedItemHashCode != 0)
        {
            // Get the last item.
            T? item = (from t in Items
                        where t.GetHashCode() == _selectedItemHashCode
                        select t).FirstOrDefault();

            if (item != null)
            {
                int index = IndexOf(item);
                base[index] = _selectedItem; // Replace the item in the list with itself to trigger updates.
                OnRaiseSelectedItemChangingEvent(_selectedItem);
            }
        }

        _updating = false; // All done.
    }

    /// <summary>
    /// Raises the <see cref="SelectedItemChanging"/> event.
    /// </summary>
    /// <param name="args">The item that is about to be selected.</param>
    protected virtual void OnRaiseSelectedItemChangingEvent(T args)
    {
        SelectedItemChanging?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the <see cref="SelectedItemChanged"/> event.
    /// </summary>
    /// <param name="args">The item that has been selected.</param>
    protected virtual void OnRaiseSelectedItemChangedEvent(T args)
    {
        SelectedItemChanged?.Invoke(this, args);
    }
}
