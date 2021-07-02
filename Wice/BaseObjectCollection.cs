using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Wice
{
    public class BaseObjectCollection<T> : BaseObject, INotifyCollectionChanged, IList<T>, IList where T : BaseObject // IList is good for property grid support
    {
        private readonly List<T> _list = new List<T>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public BaseObjectCollection(int maxChildrenCount = int.MaxValue)
        {
            MaxChildrenCount = maxChildrenCount;
        }

        public int MaxChildrenCount { get; }
        public T this[int index]
        {
            get => _list[index];
            set
            {
                var existing = _list[index];
                if (existing == null && value == null)
                    return;

                if (existing.Equals(value))
                    return;

                OnPropertyChanging(this, new PropertyChangingEventArgs(index.ToString()));
                _list[index] = value;
                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, existing, index));
                OnPropertyChanged(this, new PropertyChangedEventArgs(index.ToString()));
            }
        }

        public int Count => _list.Count;
        public bool IsReadOnly { get; set; }

        bool IList.IsFixedSize => false;
        object ICollection.SyncRoot => null;
        bool ICollection.IsSynchronized => true;
        object IList.this[int index] { get => this[index]; set => this[index] = (T)value; }

        public override string ToString() => base.ToString() + " Count: " + Count;
        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(sender, e);

        protected virtual void ProtectedAdd(T item, bool checkMaxChildrenCount)
        {
            if (checkMaxChildrenCount && Count == MaxChildrenCount)
                throw new WiceException("0002: Collection has a maximum of " + MaxChildrenCount + " children.");

            OnItemsChanged(() =>
            {
                _list.Add(item);
                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _list.Count - 1));
            });
        }

        public void InsertAfter(T after, T item)
        {
            var index = _list.IndexOf(after);
            if (index < 0)
            {
                Add(item);
                return;
            }

            Insert(index, item);
        }

        public void InsertBefore(T before, T item)
        {
            var index = _list.IndexOf(before);
            if (index < 0)
            {
                Insert(0, item);
                return;
            }

            Insert(index, item);
        }

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (IsReadOnly)
                throw new NotSupportedException();

            ProtectedAdd(item, true);
        }

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
                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        public void Sort() => _list.Sort();
        public void Sort(IComparer<T> comparer) => _list.Sort(comparer);

        public void Clear()
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            ProtectedClear();
        }

        public bool Contains(T item) => _list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int IndexOf(T item) => _list.IndexOf(item);

        public void Insert(int index, T item)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (IsReadOnly)
                throw new NotSupportedException();

            ProtectedInsert(index, item);
        }

        protected virtual void ProtectedInsert(int index, T item)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (Count == MaxChildrenCount)
                throw new WiceException("0004: Collection has a maximum of " + MaxChildrenCount + " children.");

            if (_list.Contains(item))
                throw new WiceException("0005: Element named '" + item.Name + "' of type '" + item.GetType().Name + "' has already been added as a children.");

            OnItemsChanged(() =>
            {
                _list.Insert(index, item);
                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            });
        }

        public bool Remove(T item)
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            return ProtectedRemove(item);
        }

        protected virtual bool ProtectedRemove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
                return false;

            OnItemsChanged(() =>
            {
                _list.RemoveAt(index);
                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            });
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (IsReadOnly)
                throw new NotSupportedException();

            ProtectedRemoveAt(index);
        }

        protected virtual void ProtectedRemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            OnItemsChanged(() =>
            {
                var item = _list[index];
                _list.RemoveAt(index);
                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            });
        }

        private void OnItemsChanged(Action action)
        {
            OnPropertyChanging(this, new PropertyChangingEventArgs(nameof(Count)));
            action();
            OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        int IList.Add(object value)
        {
            var count = Count;
            Add((T)value);
            return count;
        }

        bool IList.Contains(object value) => Contains((T)value);
        int IList.IndexOf(object value) => IndexOf((T)value);
        void IList.Insert(int index, object value) => Insert(index, (T)value);
        void IList.Remove(object value) => Remove((T)value);
        void ICollection.CopyTo(Array array, int index) => CopyTo((T[])array, index);
    }
}
