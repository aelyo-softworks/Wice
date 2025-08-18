namespace Wice.Utilities;

/// <summary>
/// A high-performance, thread-safe hash set that supports concurrent <see cref="Add(T)"/>, <see cref="TryRemove(T)"/>,
/// and <see cref="Contains(T)"/> operations with lock striping.
/// </summary>
/// <typeparam name="T">The type of elements stored in the set.</typeparam>
/// <remarks>
/// <para>
/// This implementation stripes buckets across a set of locks to provide scalable concurrency. Each mutation acquires only
/// the lock for the target bucket. Reads via <see cref="Contains(T)"/> are lock-free. The set grows automatically when
/// its per-lock count exceeds an adaptive budget.
/// </para>
/// <para>
/// Enumeration is weakly consistent: <see cref="GetEnumerator"/> does not acquire locks and may reflect items added or removed
/// during enumeration. It never throws due to concurrent modifications, but it does not represent a snapshot.
/// </para>
/// <para>
/// <see cref="IEqualityComparer{T}"/> is used for hashing and equality. If no comparer is provided, <see cref="EqualityComparer{T}.Default"/> is used.
/// A <c>null</c> item (for reference or nullable value types) hashes to 0.
/// </para>
/// </remarks>
public class ConcurrentHashSet<T> : IReadOnlyCollection<T>, ICollection<T>
{
    private const int _defaultCapacity = 31;
    private const int _maxLockNumber = 1024;

    private volatile Tables _tables;
    private readonly IEqualityComparer<T> _comparer;
    private readonly bool _growLockArray;
    private int _budget;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class with a default concurrency level
    /// based on <see cref="Environment.ProcessorCount"/> and an initial capacity of 31 buckets.
    /// </summary>
    public ConcurrentHashSet()
        : this(Environment.ProcessorCount, _defaultCapacity, true, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class with the specified concurrency level and capacity.
    /// </summary>
    /// <param name="concurrencyLevel">The estimated number of concurrent writers. Must be at least 1.</param>
    /// <param name="capacity">The initial number of buckets. Must be non-negative. Will be raised to <paramref name="concurrencyLevel"/> if smaller.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="concurrencyLevel"/> is less than 1 or <paramref name="capacity"/> is negative.</exception>
    public ConcurrentHashSet(int concurrencyLevel, int capacity)
        : this(concurrencyLevel, capacity, false, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class that contains elements copied from the specified collection,
    /// using the default equality comparer.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
    public ConcurrentHashSet(IEnumerable<T> collection)
        : this(collection, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class using the specified equality comparer.
    /// </summary>
    /// <param name="comparer">The equality comparer to use for the set, or <c>null</c> for <see cref="EqualityComparer{T}.Default"/>.</param>
    public ConcurrentHashSet(IEqualityComparer<T>? comparer)
        : this(Environment.ProcessorCount, _defaultCapacity, true, comparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class that contains elements copied from the specified collection,
    /// using the specified equality comparer.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <param name="comparer">The equality comparer to use for the set, or <c>null</c> for <see cref="EqualityComparer{T}.Default"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
    public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer)
        : this(comparer)
    {
        ExceptionExtensions.ThrowIfNull(collection, nameof(collection));
        InitializeFromCollection(collection);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class that contains elements copied from the specified collection,
    /// using the specified concurrency level and equality comparer.
    /// </summary>
    /// <param name="concurrencyLevel">The estimated number of concurrent writers. Must be at least 1.</param>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <param name="comparer">The equality comparer to use for the set.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="concurrencyLevel"/> is less than 1.</exception>
    public ConcurrentHashSet(int concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer)
        : this(concurrencyLevel, _defaultCapacity, false, comparer)
    {
        ExceptionExtensions.ThrowIfNull(collection, nameof(collection));
        InitializeFromCollection(collection);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class with the specified concurrency level, capacity, and equality comparer.
    /// </summary>
    /// <param name="concurrencyLevel">The estimated number of concurrent writers. Must be at least 1.</param>
    /// <param name="capacity">The initial number of buckets. Must be non-negative. Will be raised to <paramref name="concurrencyLevel"/> if smaller.</param>
    /// <param name="comparer">The equality comparer to use for the set.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="concurrencyLevel"/> is less than 1 or <paramref name="capacity"/> is negative.</exception>
    public ConcurrentHashSet(int concurrencyLevel, int capacity, IEqualityComparer<T> comparer)
        : this(concurrencyLevel, capacity, false, comparer)
    {
    }

    private ConcurrentHashSet(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<T>? comparer)
    {
#if NETFRAMEWORK
        if (concurrencyLevel < 1) throw new ArgumentOutOfRangeException(nameof(concurrencyLevel));
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
#else
        ArgumentOutOfRangeException.ThrowIfLessThan(concurrencyLevel, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
#endif

        // The capacity should be at least as large as the concurrency level. Otherwise, we would have locks that don't guard
        // any buckets.
        if (capacity < concurrencyLevel)
        {
            capacity = concurrencyLevel;
        }

        var locks = new object[concurrencyLevel];
        for (var i = 0; i < locks.Length; i++)
        {
            locks[i] = new object();
        }

        var countPerLock = new int[locks.Length];
        var buckets = new Node[capacity];
        _tables = new Tables(buckets, locks, countPerLock);

        _growLockArray = growLockArray;
        _budget = buckets.Length / locks.Length;
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    /// <summary>
    /// Gets the number of elements contained in the set.
    /// </summary>
    /// <remarks>
    /// Acquires all internal locks to compute the total count.
    /// </remarks>
    public int Count
    {
        get
        {
            var count = 0;
            var acquiredLocks = 0;
            try
            {
                AcquireAllLocks(ref acquiredLocks);
                for (var i = 0; i < _tables.CountPerLock.Length; i++)
                {
                    count += _tables.CountPerLock[i];
                }
            }
            finally
            {
                ReleaseLocks(0, acquiredLocks);
            }
            return count;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the set contains no elements.
    /// </summary>
    /// <remarks>
    /// Acquires all internal locks to check per-lock counts.
    /// </remarks>
    public bool IsEmpty
    {
        get
        {
            var acquiredLocks = 0;
            try
            {
                AcquireAllLocks(ref acquiredLocks);
                for (var i = 0; i < _tables.CountPerLock.Length; i++)
                {
                    if (_tables.CountPerLock[i] != 0)
                        return false;
                }
            }
            finally
            {
                ReleaseLocks(0, acquiredLocks);
            }
            return true;
        }
    }

    /// <summary>
    /// Attempts to add the specified item to the set.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns><c>true</c> if the item was added to the set; <c>false</c> if it was already present.</returns>
    public bool Add(T item) => AddInternal(item, GetHashCode(item), true);

    /// <summary>
    /// Removes all items from the set.
    /// </summary>
    /// <remarks>
    /// Acquires all internal locks and replaces the bucket table. The lock array is preserved.
    /// </remarks>
    public void Clear()
    {
        var locksAcquired = 0;
        try
        {
            AcquireAllLocks(ref locksAcquired);
            var newTables = new Tables(new Node[_defaultCapacity], _tables.Locks, new int[_tables.CountPerLock.Length]);
            _tables = newTables;
            _budget = Math.Max(1, newTables.Buckets.Length / newTables.Locks.Length);
        }
        finally
        {
            ReleaseLocks(0, locksAcquired);
        }
    }

    /// <summary>
    /// Determines whether the set contains a specific element.
    /// </summary>
    /// <param name="item">The item to locate in the set.</param>
    /// <returns><c>true</c> if the item is found; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This operation is lock-free and may observe concurrent updates.
    /// </remarks>
    public bool Contains(T item)
    {
        var hashcode = GetHashCode(item);
        var tables = _tables;
        var bucketNo = GetBucket(hashcode, tables.Buckets.Length);
        var current = Volatile.Read(ref tables.Buckets[bucketNo]);

        while (current != null)
        {
            if (hashcode == current.Hashcode && _comparer.Equals(current.Item, item))
                return true;

            current = current.Next;
        }
        return false;
    }

    /// <summary>
    /// Attempts to remove the specified item from the set.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><c>true</c> if the element was found and removed; otherwise, <c>false</c>.</returns>
    public bool TryRemove(T item)
    {
        var hashcode = GetHashCode(item);
        while (true)
        {
            var tables = _tables;
            GetBucketAndLockNo(hashcode, out int bucketNo, out int lockNo, tables.Buckets.Length, tables.Locks.Length);
            lock (tables.Locks[lockNo])
            {
                if (tables != _tables)
                    continue;

                Node? previous = null;
                for (var current = tables.Buckets[bucketNo]; current != null; current = current.Next)
                {
                    if (hashcode == current.Hashcode && _comparer.Equals(current.Item, item))
                    {
                        if (previous == null)
                        {
                            Volatile.Write(ref tables.Buckets[bucketNo], current.Next);
                        }
                        else
                        {
                            previous.Next = current.Next;
                        }

                        tables.CountPerLock[lockNo]--;
                        return true;
                    }
                    previous = current;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the set.
    /// </summary>
    /// <remarks>
    /// Enumeration is weakly consistent and does not block concurrent writers. It may include items added or exclude items removed during enumeration.
    /// </remarks>
    /// <returns>An enumerator over the items in the set.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        var buckets = _tables.Buckets;
        for (var i = 0; i < buckets.Length; i++)
        {
            var current = Volatile.Read(ref buckets[i]);
            while (current != null)
            {
                yield return current.Item;
                current = current.Next;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    bool ICollection<T>.Remove(T item) => TryRemove(item);
    void ICollection<T>.Add(T item) => Add(item);
    bool ICollection<T>.IsReadOnly => false;
    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        ExceptionExtensions.ThrowIfNull(array, nameof(array));
#if NETFRAMEWORK
        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
#else
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
#endif
        var locksAcquired = 0;
        try
        {
            AcquireAllLocks(ref locksAcquired);
            var count = 0;
            for (var i = 0; i < _tables.Locks.Length && count >= 0; i++)
            {
                count += _tables.CountPerLock[i];
            }

            if (array.Length - count < arrayIndex || count < 0)
                throw new ArgumentException(null, nameof(arrayIndex));

            CopyToItems(array, arrayIndex);
        }
        finally
        {
            ReleaseLocks(0, locksAcquired);
        }
    }

    private int GetHashCode(T? item)
    {
        if (item is null)
            return 0;

        return _comparer.GetHashCode(item);
    }

    private void InitializeFromCollection(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            AddInternal(item, GetHashCode(item), false);
        }

        if (_budget == 0)
        {
            _budget = _tables.Buckets.Length / _tables.Locks.Length;
        }
    }

    private bool AddInternal(T item, int hashcode, bool acquireLock)
    {
        while (true)
        {
            var tables = _tables;
            GetBucketAndLockNo(hashcode, out int bucketNo, out int lockNo, tables.Buckets.Length, tables.Locks.Length);
            var resizeDesired = false;
            var lockTaken = false;

            try
            {
                if (acquireLock)
                {
                    System.Threading.Monitor.Enter(tables.Locks[lockNo], ref lockTaken);
                }

                if (tables != _tables)
                    continue;

                Node? previous = null;
                for (var current = tables.Buckets[bucketNo]; current != null; current = current.Next)
                {
                    if (hashcode == current.Hashcode && _comparer.Equals(current.Item, item))
                    {
                        return false;
                    }
                    previous = current;
                }

                Volatile.Write(ref tables.Buckets[bucketNo], new Node(item, hashcode, tables.Buckets[bucketNo]));
                checked
                {
                    tables.CountPerLock[lockNo]++;
                }

                if (tables.CountPerLock[lockNo] > _budget)
                {
                    resizeDesired = true;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    System.Threading.Monitor.Exit(tables.Locks[lockNo]);
                }
            }

            if (resizeDesired)
            {
                GrowTable(tables);
            }
            return true;
        }
    }

    private static int GetBucket(int hashcode, int bucketCount)
    {
        var bucketNo = (hashcode & 0x7fffffff) % bucketCount;
        return bucketNo;
    }

    private static void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount, int lockCount)
    {
        bucketNo = (hashcode & 0x7fffffff) % bucketCount;
        lockNo = bucketNo % lockCount;
    }

    private void GrowTable(Tables tables)
    {
        const int maxArrayLength = 0X7FEFFFFF;
        var locksAcquired = 0;
        try
        {
            // Acquire the first lock to serialize table growth.
            AcquireLocks(0, 1, ref locksAcquired);
            if (tables != _tables)
                return;

            long approxCount = 0;
            for (var i = 0; i < tables.CountPerLock.Length; i++)
            {
                approxCount += tables.CountPerLock[i];
            }

            // If we are under-utilized, increase budget instead of growing.
            if (approxCount < tables.Buckets.Length / 4)
            {
                _budget = 2 * _budget;
                if (_budget < 0)
                {
                    _budget = int.MaxValue;
                }
                return;
            }

            var newLength = 0;
            var maximizeTableSize = false;
            try
            {
                checked
                {
                    newLength = tables.Buckets.Length * 2 + 1;
                    // Prime-ish avoidance for better distribution.
                    while (newLength % 3 == 0 || newLength % 5 == 0 || newLength % 7 == 0)
                    {
                        newLength += 2;
                    }

                    if (newLength > maxArrayLength)
                    {
                        maximizeTableSize = true;
                    }
                }
            }
            catch (OverflowException)
            {
                maximizeTableSize = true;
            }

            if (maximizeTableSize)
            {
                newLength = maxArrayLength;
                _budget = int.MaxValue;
            }

            // Acquire remaining locks to rebuild.
            AcquireLocks(1, tables.Locks.Length, ref locksAcquired);
            var newLocks = tables.Locks;
            if (_growLockArray && tables.Locks.Length < _maxLockNumber)
            {
                newLocks = new object[tables.Locks.Length * 2];
                Array.Copy(tables.Locks, 0, newLocks, 0, tables.Locks.Length);
                for (var i = tables.Locks.Length; i < newLocks.Length; i++)
                {
                    newLocks[i] = new object();
                }
            }

            var newBuckets = new Node[newLength];
            var newCountPerLock = new int[newLocks.Length];

            for (var i = 0; i < tables.Buckets.Length; i++)
            {
                var current = tables.Buckets[i];
                while (current != null)
                {
                    var next = current.Next;
                    GetBucketAndLockNo(current.Hashcode, out int newBucketNo, out int newLockNo, newBuckets.Length, newLocks.Length);
                    newBuckets[newBucketNo] = new Node(current.Item, current.Hashcode, newBuckets[newBucketNo]);
                    checked
                    {
                        newCountPerLock[newLockNo]++;
                    }
                    current = next;
                }
            }

            _budget = Math.Max(1, newBuckets.Length / newLocks.Length);
            _tables = new Tables(newBuckets, newLocks, newCountPerLock);
        }
        finally
        {
            ReleaseLocks(0, locksAcquired);
        }
    }

    private void AcquireAllLocks(ref int locksAcquired)
    {
        AcquireLocks(0, 1, ref locksAcquired);
        AcquireLocks(1, _tables.Locks.Length, ref locksAcquired);
    }

    private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
    {
        var locks = _tables.Locks;
        for (var i = fromInclusive; i < toExclusive; i++)
        {
            var lockTaken = false;
            try
            {
                System.Threading.Monitor.Enter(locks[i], ref lockTaken);
            }
            finally
            {
                if (lockTaken)
                {
                    locksAcquired++;
                }
            }
        }
    }

    private void ReleaseLocks(int fromInclusive, int toExclusive)
    {
        for (var i = fromInclusive; i < toExclusive; i++)
        {
            System.Threading.Monitor.Exit(_tables.Locks[i]);
        }
    }

    private void CopyToItems(T[] array, int index)
    {
        var buckets = _tables.Buckets;
        for (var i = 0; i < buckets.Length; i++)
        {
            for (var current = buckets[i]; current != null; current = current.Next)
            {
                array[index] = current.Item;
                index++;
            }
        }
    }

    private sealed class Tables(Node[] buckets, object[] locks, int[] countPerLock)
    {
        /// <summary>The bucket array containing linked lists of nodes.</summary>
        public readonly Node[] Buckets = buckets;
        /// <summary>The array of locks used for lock striping.</summary>
        public readonly object[] Locks = locks;
        /// <summary>The number of elements guarded by each lock.</summary>
        public volatile int[] CountPerLock = countPerLock;
    }

    private sealed class Node(T item, int hashcode, Node next)
    {
        /// <summary>The stored item.</summary>
        public readonly T Item = item;
        /// <summary>The cached hash code for the item.</summary>
        public readonly int Hashcode = hashcode;
        /// <summary>Pointer to the next node in the chain.</summary>
        public volatile Node Next = next;
    }
}
