using System;

namespace Wice
{
    public interface IValueable
    {
        event EventHandler<ValueEventArgs> ValueChanged;

        object Value { get; }
        bool CanChangeValue { get; set; }
        bool TrySetValue(object value);
    }
}
