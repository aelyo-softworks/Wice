using System;

namespace Wice
{
    public class Thumb : RoundedRectangle
    {
        public event EventHandler<MouseDragEventArgs> DragStarted;
        public event EventHandler<MouseDragEventArgs> DragDelta;
        public event EventHandler<MouseDragEventArgs> DragCompleted;

        protected virtual void OnDragStarted(object sender, MouseDragEventArgs e) => DragStarted?.Invoke(sender, e);
        protected virtual void OnDragDelta(object sender, MouseDragEventArgs e) => DragDelta?.Invoke(sender, e);
        protected virtual void OnDragCompleted(object sender, MouseDragEventArgs e) => DragCompleted?.Invoke(sender, e);

        protected override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                var state = DragMove(e);
                OnDragStarted(this, new MouseDragEventArgs(e, state));
            }
            base.OnMouseButtonDown(sender, e);
        }

        public void CancelDrag(EventArgs e = null) => CancelDragMove(e);

        protected override void OnMouseDrag(object sender, MouseDragEventArgs e)
        {
            OnDragDelta(sender, e);
            base.OnMouseDrag(sender, e);
        }

        protected override DragState CancelDragMove(EventArgs e)
        {
            var state = base.CancelDragMove(e);
            OnDragCompleted(this, new MouseDragEventArgs(e as MouseEventArgs, state));
            return state;
        }
    }
}
