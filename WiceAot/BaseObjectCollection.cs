namespace Wice;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

/// <summary>
/// A list-like collection of <see cref="BaseObject"/> descendants that raises both
/// <see cref="INotifyCollectionChanged.CollectionChanged"/> and property change notifications.
/// </summary>
/// <typeparam name="T">
/// The element type, constrained to <see cref="BaseObject"/> to integrate with the host change plumbing.
/// </typeparam>
public class BaseObjectCollection<T>(int maxChildrenCount = int.MaxValue) : BaseObject, INotifyCollectionChanged, IList<T>, IList where T : BaseObject // IList is good for property grid support
{
    private readonly List<T> _list = [];

    // speed-up if many many objects
    // private readonly Dictionary<T, int> _dic = [];

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Gets the maximum allowed number of elements in this collection.
    /// </summary>
    public int MaxChildrenCount { get; } = maxChildrenCount;

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">Zero-based index of the element to get or set.</param>
    public T this[int index]
    {
        get => _list[index];
        set
        {
            var existing = _list[index];
            if (existing == null && value == null)
                return;

            if (existing != null && existing.Equals(value))
                return;

            OnPropertyChanging(this, new PropertyChangingEventArgs(index.ToString()));
            _list[index] = value;
            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, existing, index));
            OnPropertyChanged(this, new PropertyChangedEventArgs(index.ToString()));
        }
    }

    /// <summary>
    /// Gets the number of elements contained in the collection.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Gets or sets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <inheritdoc />
    bool IList.IsFixedSize => false;

    /// <inheritdoc />
    object ICollection.SyncRoot => null!;

    /// <inheritdoc />
    bool ICollection.IsSynchronized => true;

    /// <inheritdoc />
    object? IList.this[int index] { get => this[index]; set => this[index] = (T)value!; }

    /// <inheritdoc/>
    public override string ToString() => base.ToString() + " Count: " + Count;

    /// <summary>
    /// Raises <see cref="CollectionChanged"/> with the supplied event arguments.
    /// </summary>
    /// <param name="sender">The sender to pass to event subscribers.</param>
    /// <param name="e">The event arguments describing the collection change.</param>
    protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(sender, e);

    /// <summary>
    /// Adds an item to the end of the collection without checking the <see cref="IsReadOnly"/> flag,
    /// optionally enforcing <see cref="MaxChildrenCount"/>.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="checkMaxChildrenCount">If <see langword="true"/>, throws when the collection is at capacity.</param>
    protected virtual void ProtectedAdd(T item, bool checkMaxChildrenCount)
    {
        ExceptionExtensions.ThrowIfNull(item, nameof(item));
        if (checkMaxChildrenCount && Count == MaxChildrenCount)
            throw new WiceException("0002: Collection has a maximum of " + MaxChildrenCount + " children.");

        OnItemsChanged(() =>
        {
            //_dic[item] = _list.Count;
            _list.Add(item);
            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _list.Count - 1));
        });
    }

    /// <summary>
    /// Inserts <paramref name="item"/> immediately after the first occurrence of <paramref name="after"/>.
    /// Falls back to <see cref="Add(T)"/> if <paramref name="after"/> is not found.
    /// </summary>
    /// <param name="after">The item after which to insert.</param>
    /// <param name="item">The item to insert.</param>
    public void InsertAfter(T after, T item)
    {
        var index = _list.IndexOf(after);
        if (index < 0)
        //if (!_dic.TryGetValue(after, out var index))
        {
            Add(item);
            return;
        }

        Insert(index, item);
    }

    /// <summary>
    /// Inserts <paramref name="item"/> immediately before the first occurrence of <paramref name="before"/>.
    /// Falls back to inserting at index 0 if <paramref name="before"/> is not found.
    /// </summary>
    /// <param name="before">The item before which to insert.</param>
    /// <param name="item">The item to insert.</param>
    public void InsertBefore(T before, T item)
    {
        var index = _list.IndexOf(before);
        if (index < 0)
        //if (!_dic.TryGetValue(before, out var index))
        {
            Insert(0, item);
            return;
        }

        Insert(index, item);
    }

    /// <summary>
    /// Adds an item to the end of the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        ExceptionExtensions.ThrowIfNull(item, nameof(item));
        if (IsReadOnly)
            throw new NotSupportedException();

        ProtectedAdd(item, true);
    }

    /// <summary>
    /// Removes all elements from the collection without honoring <see cref="IsReadOnly"/>.
    /// </summary>
    protected virtual void ProtectedClear()
    {
        if (_list.Count == 0)
            return;

        OnItemsChanged(() =>
        {
            var count = _list.Count;
            for (var i = 0; i < count; i++)
            {
                RemoveAt(_list.Count - 1);
            }
            //_dic.Clear();
            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        });
    }

    /// <summary>
    /// Sorts the elements in the entire collection using the default comparer for <typeparamref name="T"/>.
    /// </summary>
    public void Sort() => _list.Sort();

    /// <summary>
    /// Sorts the elements in the entire collection using the specified comparer.
    /// </summary>
    /// <param name="comparer">The comparer to use, or <see langword="null"/> to use the default comparer.</param>
    public void Sort(IComparer<T> comparer) => _list.Sort(comparer);

    /// <summary>
    /// Removes all elements from the collection.
    /// </summary>
    public void Clear()
    {
        if (IsReadOnly)
            throw new NotSupportedException();

        ProtectedClear();
    }

    /// <summary>
    /// Determines whether the collection contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate.</param>
    /// <returns><see langword="true"/> if found; otherwise, <see langword="false"/>.</returns>
    public bool Contains(T item) => _list.Contains(item);
    //public bool Contains(T item) => _dic.ContainsKey(item);

    /// <summary>
    /// Copies the elements of the collection to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the first occurrence.
    /// </summary>
    /// <param name="item">The object to locate.</param>
    /// <returns>The zero-based index if found; otherwise, -1.</returns>
    public int IndexOf(T item) => _list.IndexOf(item);
    //public int IndexOf(T item)
    //{
    //    if (!_dic.TryGetValue(item, out var index))
    //        return -1;
    //
    //    return index;
    //}

    /// <summary>
    /// Inserts an item to the collection at the specified index.
    /// </summary>
    /// <param name="index">Zero-based index at which <paramref name="item"/> should be inserted.</param>
    /// <param name="item">The item to insert.</param>
    public void Insert(int index, T item)
    {
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (IsReadOnly)
            throw new NotSupportedException();

        ProtectedInsert(index, item);
    }

    /// <summary>
    /// Inserts an item at a specific index without honoring <see cref="IsReadOnly"/>.
    /// </summary>
    /// <param name="index">Zero-based insertion index.</param>
    /// <param name="item">The item to insert.</param>
    protected virtual void ProtectedInsert(int index, T item)
    {
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (Count == MaxChildrenCount)
            throw new WiceException("0004: Collection has a maximum of " + MaxChildrenCount + " children.");

        if (Contains(item))
            throw new WiceException("0005: Element named '" + item.Name + "' of type '" + item.GetType().Name + "' has already been added as a children.");

        OnItemsChanged(() =>
        {
            //_dic[item] = index;
            _list.Insert(index, item);
            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        });
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><see langword="true"/> if the item was removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(T item)
    {
        if (IsReadOnly)
            throw new NotSupportedException();

        return ProtectedRemove(item);
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the collection without honoring <see cref="IsReadOnly"/>.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><see langword="true"/> if the item was removed; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ProtectedRemove(T item)
    {
        var index = IndexOf(item);
        if (index < 0)
            return false;

        OnItemsChanged(() =>
        {
            //_dic.Remove(item);
            _list.RemoveAt(index);
            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        });
        return true;
    }

    /// <summary>
    /// Removes the element at the specified index of the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (IsReadOnly)
            throw new NotSupportedException();

        ProtectedRemoveAt(index);
    }

    /// <summary>
    /// Removes the element at the specified index without honoring <see cref="IsReadOnly"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    protected virtual void ProtectedRemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        OnItemsChanged(() =>
        {
            var item = _list[index];
            //_dic.Remove(item);
            _list.RemoveAt(index);
            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        });
    }

    /// <summary>
    /// Wraps an action that changes the items to raise <see cref="INotifyPropertyChanging.PropertyChanging"/> and
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> for the <see cref="Count"/> property.
    /// </summary>
    /// <param name="action">The action that mutates the collection.</param>
    private void OnItemsChanged(Action action)
    {
        OnPropertyChanging(this, new PropertyChangingEventArgs(nameof(Count)));
        action();
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(Count)));
    }

    /// <inheritdoc />
    int IList.Add(object? value)
    {
        var count = Count;
        Add((T)value!);
        return count;
    }

    /// <inheritdoc />
    bool IList.Contains(object? value) => Contains((T)value!);

    /// <inheritdoc />
    int IList.IndexOf(object? value) => IndexOf((T)value!);

    /// <inheritdoc />
    void IList.Insert(int index, object? value) => Insert(index, (T)value!);

    /// <inheritdoc />
    void IList.Remove(object? value) => Remove((T)value!);

    /// <inheritdoc />
    void ICollection.CopyTo(Array array, int index) => CopyTo((T[])array, index);
}
