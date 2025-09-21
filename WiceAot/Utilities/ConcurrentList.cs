namespace Wice.Utilities;

/// <summary>
/// A coarse-grained, thread-safe wrapper around <see cref="List{T}"/>.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public class ConcurrentList<T> : IList<T>, IReadOnlyList<T>
{
    /// <summary>
    /// The synchronization object used to guard all accesses to the underlying list.
    /// </summary>
    protected readonly object SyncObject = new();

    /// <summary>
    /// Initializes a new, empty instance of the <see cref="ConcurrentList{T}"/> class.
    /// </summary>
    public ConcurrentList()
    {
        List = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class with the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial number of elements the list can contain.</param>
    public ConcurrentList(int capacity)
    {
        List = new List<T>(capacity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    public ConcurrentList(IEnumerable<T> collection)
    {
        ExceptionExtensions.ThrowIfNull(collection, nameof(collection));
        List = [.. collection];
    }

    /// <summary>
    /// The underlying list. Access must be synchronized using <see cref="SyncObject"/>.
    /// </summary>
    protected List<T> List { get; private set; }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    public virtual T this[int index]
    {
        get
        {
            lock (SyncObject)
            {
                return List[index];
            }
        }
        set
        {
            lock (SyncObject)
            {
                List[index] = value;
            }
        }
    }

    /// <summary>
    /// Gets the number of elements contained in the list.
    /// </summary>
    public virtual int Count
    {
        get
        {
            lock (SyncObject)
            {
                return List.Count;
            }
        }
    }

    bool ICollection<T>.IsReadOnly => false;

    /// <summary>
    /// Adds an item to the list.
    /// </summary>
    /// <param name="item">The object to add.</param>
    public virtual void Add(T item)
    {
        lock (SyncObject)
        {
            List.Add(item);
        }
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the list.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added.</param>
    public virtual void AddRange(IEnumerable<T> collection)
    {
        lock (SyncObject)
        {
            List.AddRange(collection);
        }
    }

    /// <summary>
    /// Sorts the elements in the entire list using the default comparer.
    /// </summary>
    public virtual void Sort()
    {
        lock (SyncObject)
        {
            List.Sort();
        }
    }

    /// <summary>
    /// Sorts the elements in the entire list using the specified comparer.
    /// </summary>
    /// <param name="comparer">The comparer to use when comparing elements.</param>
    public virtual void Sort(IComparer<T> comparer)
    {
        lock (SyncObject)
        {
            List.Sort(comparer);
        }
    }

    /// <summary>
    /// Removes all elements from the list.
    /// </summary>
    public virtual void Clear()
    {
        lock (SyncObject)
        {
            List.Clear();
        }
    }

    /// <summary>
    /// Copies the elements to a new array and optionally clears the list.
    /// </summary>
    /// <param name="clear">If true, clears the list after copying.</param>
    /// <returns>An array containing copies of the elements.</returns>
    public virtual T[] ToArray(bool clear = false)
    {
        lock (SyncObject)
        {
            var array = List.ToArray();
            if (clear)
            {
                List.Clear();
            }
            return array;
        }
    }

    /// <summary>
    /// Copies the elements to a new list or returns the underlying list and resets the internal storage.
    /// </summary>
    /// <param name="clear">
    /// If true, returns the current underlying list instance and replaces it with an empty one;
    /// if false, returns a shallow copy.
    /// </param>
    /// <returns>A list containing the elements.</returns>
    public virtual List<T> ToList(bool clear = false)
    {
        lock (SyncObject)
        {
            List<T> list;
            if (clear)
            {
                list = List;
                List = [];
            }
            else
            {
                list = [.. List];
            }
            return list;
        }
    }

    /// <summary>
    /// Determines whether the list contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate.</param>
    /// <returns>true if item is found; otherwise, false.</returns>
    public virtual bool Contains(T item)
    {
        lock (SyncObject)
        {
            return List.Contains(item);
        }
    }

    /// <summary>
    /// Copies the entire list to a compatible one-dimensional array, starting at the specified array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public virtual void CopyTo(T[] array, int arrayIndex)
    {
        lock (SyncObject)
        {
            List.CopyTo(array, arrayIndex);
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a snapshot of the list.
    /// </summary>
    /// <returns>
    /// An enumerator over an array snapshot of the current contents. Changes after enumeration starts are not reflected.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns a generic enumerator that iterates through a snapshot of the list.
    /// </summary>
    /// <returns>
    /// An enumerator over an array snapshot of the current contents. Changes after enumeration starts are not reflected.
    /// </returns>
    public IEnumerator<T> GetEnumerator()
    {
        lock (SyncObject)
        {
            return ToArray().Cast<T>().GetEnumerator();
        }
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the first occurrence.
    /// </summary>
    /// <param name="item">The object to locate.</param>
    /// <returns>The zero-based index of the first occurrence, if found; otherwise, -1.</returns>
    public virtual int IndexOf(T item)
    {
        lock (SyncObject)
        {
            return List.IndexOf(item);
        }
    }

    /// <summary>
    /// Inserts an element into the list at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The object to insert.</param>
    public virtual void Insert(int index, T item)
    {
        lock (SyncObject)
        {
            List.Insert(index, item);
        }
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the list.
    /// </summary>
    /// <param name="item">The object to remove.</param>
    /// <returns>true if item is successfully removed; otherwise, false.</returns>
    public virtual bool Remove(T item)
    {
        lock (SyncObject)
        {
            return List.Remove(item);
        }
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate and returns the first occurrence.
    /// </summary>
    /// <param name="match">The predicate that defines the conditions of the element to search for.</param>
    /// <returns>The first element that matches the predicate, or the default value for <typeparamref name="T"/> if no match is found.</returns>
    public virtual T? Find(Func<T, bool> match)
    {
        lock (SyncObject)
        {
            return List.Find(t => match(t));
        }
    }

    /// <summary>
    /// Retrieves all the elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The predicate that defines the conditions of the elements to search for.</param>
    /// <returns>A list containing all the elements that match the specified predicate.</returns>
    public virtual List<T> FindAll(Func<T, bool> match)
    {
        lock (SyncObject)
        {
            return List.FindAll(t => match(t));
        }
    }

    /// <summary>
    /// Removes all the elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The predicate that defines the conditions of the elements to remove.</param>
    /// <returns>The number of elements removed.</returns>
    public virtual int RemoveAll(Func<T, bool> match)
    {
        lock (SyncObject)
        {
            return List.RemoveAll(t => match(t));
        }
    }

    /// <summary>
    /// Removes the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    public virtual void RemoveAt(int index)
    {
        lock (SyncObject)
        {
            List.RemoveAt(index);
        }
    }
}
