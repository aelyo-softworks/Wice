using System;

namespace Wice
{
    public interface ISelectable
    {
        event EventHandler<ValueEventArgs<bool>> IsSelectedChanged;

        bool RaiseIsSelectedChanged { get; set; }
        bool IsSelected { get; set; }
    }
}
