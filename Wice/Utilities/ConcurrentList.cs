using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wice.Utilities
{
    public class ConcurrentList<T> : IList<T>, IReadOnlyList<T>
    {
        protected readonly object SyncObject = new object();

        public ConcurrentList()
        {
            List = new List<T>();
        }

        public ConcurrentList(int capacity)
        {
            List = new List<T>(capacity);
        }

        public ConcurrentList(IEnumerable<T> collection)
        {
            List = new List<T>(collection);
        }

        protected List<T> List { get; private set; }

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

        public virtual void Add(T item)
        {
            lock (SyncObject)
            {
                List.Add(item);
            }
        }

        public virtual void AddRange(IEnumerable<T> collection)
        {
            lock (SyncObject)
            {
                List.AddRange(collection);
            }
        }

        public virtual void Sort()
        {
            lock (SyncObject)
            {
                List.Sort();
            }
        }

        public virtual void Sort(IComparer<T> comparer)
        {
            lock (SyncObject)
            {
                List.Sort(comparer);
            }
        }

        public virtual void Clear()
        {
            lock (SyncObject)
            {
                List.Clear();
            }
        }

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

        public virtual List<T> ToList(bool clear = false)
        {
            lock (SyncObject)
            {
                List<T> list;
                if (clear)
                {
                    list = List;
                    List = new List<T>();
                }
                else
                {
                    list = List.ToList();
                }
                return list;
            }
        }

        public virtual bool Contains(T item)
        {
            lock (SyncObject)
            {
                return List.Contains(item);
            }
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncObject)
            {
                List.CopyTo(array, arrayIndex);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            lock (SyncObject)
            {
                return ToArray().Cast<T>().GetEnumerator();
            }
        }

        public virtual int IndexOf(T item)
        {
            lock (SyncObject)
            {
                return List.IndexOf(item);
            }
        }

        public virtual void Insert(int index, T item)
        {
            lock (SyncObject)
            {
                List.Insert(index, item);
            }
        }

        public virtual bool Remove(T item)
        {
            lock (SyncObject)
            {
                return List.Remove(item);
            }
        }

        public virtual T Find(Func<T, bool> match)
        {
            lock (SyncObject)
            {
                return List.Find(t => match(t));
            }
        }

        public virtual List<T> FindAll(Func<T, bool> match)
        {
            lock (SyncObject)
            {
                return List.FindAll(t => match(t));
            }
        }

        public virtual int RemoveAll(Func<T, bool> match)
        {
            lock (SyncObject)
            {
                return List.RemoveAll(t => match(t));
            }
        }

        public virtual void RemoveAt(int index)
        {
            lock (SyncObject)
            {
                List.RemoveAt(index);
            }
        }
    }
}
