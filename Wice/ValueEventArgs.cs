using System;
using System.ComponentModel;

namespace Wice
{
    public class ValueEventArgs : CancelEventArgs
    {
        private object _value;

        public ValueEventArgs(object value, bool isValueReadOnly = true, bool isCancellable = false)
        {
            _value = value;
            IsValueReadOnly = isValueReadOnly;
            IsCancellable = isCancellable;
        }

        public virtual bool IsCancellable { get; }
        public virtual bool IsValueReadOnly { get; }
        public virtual object Value { get => _value; set { if (IsValueReadOnly) throw new ArgumentException(null, nameof(Value)); _value = value; } }
    }

    public class ValueEventArgs<T> : ValueEventArgs
    {
        public ValueEventArgs(T value, bool isValueReadOnly = true, bool isCancellable = false)
            : base(value, isValueReadOnly, isCancellable)
        {
        }

        public new T Value { get => (T)base.Value; set => base.Value = value; }
    }
}
