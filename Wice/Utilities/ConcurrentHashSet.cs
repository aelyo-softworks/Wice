using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Wice.Utilities
{
    public class ConcurrentHashSet<T> : IReadOnlyCollection<T>, ICollection<T>
    {
        private const int _defaultCapacity = 31;
        private const int _maxLockNumber = 1024;

        private readonly IEqualityComparer<T> _comparer;
        private readonly bool _growLockArray;
        private int _budget;
        private volatile Tables _tables;

        public ConcurrentHashSet()
            : this(Environment.ProcessorCount, _defaultCapacity, true, null)
        {
        }

        public ConcurrentHashSet(int concurrencyLevel, int capacity)
            : this(concurrencyLevel, capacity, false, null)
        {
        }

        public ConcurrentHashSet(IEnumerable<T> collection)
            : this(collection, null)
        {
        }

        public ConcurrentHashSet(IEqualityComparer<T> comparer)
            : this(Environment.ProcessorCount, _defaultCapacity, true, comparer)
        {
        }

        public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(comparer)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            InitializeFromCollection(collection);
        }

        public ConcurrentHashSet(int concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(concurrencyLevel, _defaultCapacity, false, comparer)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            InitializeFromCollection(collection);
        }

        public ConcurrentHashSet(int concurrencyLevel, int capacity, IEqualityComparer<T> comparer)
            : this(concurrencyLevel, capacity, false, comparer)
        {
        }

        private ConcurrentHashSet(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<T> comparer)
        {
            if (concurrencyLevel < 1) throw new ArgumentOutOfRangeException(nameof(concurrencyLevel));
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

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


        public bool Add(T item) => AddInternal(item, _comparer.GetHashCode(item), true);
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

        public bool Contains(T item)
        {
            var hashcode = _comparer.GetHashCode(item);
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

        public bool TryRemove(T item)
        {
            var hashcode = _comparer.GetHashCode(item);
            while (true)
            {
                var tables = _tables;
                GetBucketAndLockNo(hashcode, out int bucketNo, out int lockNo, tables.Buckets.Length, tables.Locks.Length);
                lock (tables.Locks[lockNo])
                {
                    if (tables != _tables)
                        continue;

                    Node previous = null;
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
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

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

        private void InitializeFromCollection(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                AddInternal(item, _comparer.GetHashCode(item), false);
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
                        Monitor.Enter(tables.Locks[lockNo], ref lockTaken);

                    if (tables != _tables)
                        continue;

                    Node previous = null;
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
                        Monitor.Exit(tables.Locks[lockNo]);
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
                AcquireLocks(0, 1, ref locksAcquired);
                if (tables != _tables)
                    return;

                long approxCount = 0;
                for (var i = 0; i < tables.CountPerLock.Length; i++)
                {
                    approxCount += tables.CountPerLock[i];
                }

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
                    Monitor.Enter(locks[i], ref lockTaken);
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
                Monitor.Exit(_tables.Locks[i]);
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

        private class Tables
        {
            public readonly Node[] Buckets;
            public readonly object[] Locks;
            public volatile int[] CountPerLock;

            public Tables(Node[] buckets, object[] locks, int[] countPerLock)
            {
                Buckets = buckets;
                Locks = locks;
                CountPerLock = countPerLock;
            }
        }

        private class Node
        {
            public readonly T Item;
            public readonly int Hashcode;
            public volatile Node Next;

            public Node(T item, int hashcode, Node next)
            {
                Item = item;
                Hashcode = hashcode;
                Next = next;
            }
        }
    }
}
