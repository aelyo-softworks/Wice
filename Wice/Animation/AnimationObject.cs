using System;

namespace Wice.Animation
{
    public abstract class AnimationObject : BaseObject
    {
        public event EventHandler<EventArgs> AttachedToParent;
        public event EventHandler<EventArgs> DetachedFromParent;

        public AnimationObject Parent { get; internal set; }

        protected internal virtual void OnDetachedFromParent(object sender, EventArgs e) => DetachedFromParent?.Invoke(sender, e);
        protected internal virtual void OnAttachedToParent(object sender, EventArgs e) => AttachedToParent?.Invoke(sender, e);
        protected internal virtual void OnChildAdded(AnimationObject child) { }
        protected internal virtual void OnChildRemoved(AnimationObject child) { }
    }
}
