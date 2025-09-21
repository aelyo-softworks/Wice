namespace Wice.Animation;

/// <summary>
/// Base type for animation graph objects that participate in a simple parent/child lifecycle.
/// </summary>
public abstract class AnimationObject : BaseObject
{
    /// <summary>
    /// Occurs when this instance has been attached to a <see cref="Parent"/>.
    /// </summary>
    public event EventHandler<EventArgs>? AttachedToParent;

    /// <summary>
    /// Occurs when this instance has been detached from its <see cref="Parent"/>.
    /// </summary>
    public event EventHandler<EventArgs>? DetachedFromParent;

    /// <summary>
    /// Gets the logical parent in the animation object tree.
    /// </summary>
    public AnimationObject? Parent { get; internal set; }

    /// <summary>
    /// Raises <see cref="DetachedFromParent"/>. Override to observe detachment.
    /// </summary>
    /// <param name="sender">The sender to surface to event handlers.</param>
    /// <param name="e">Event arguments.</param>
    protected internal virtual void OnDetachedFromParent(object? sender, EventArgs e) => DetachedFromParent?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="AttachedToParent"/>. Override to observe attachment.
    /// </summary>
    /// <param name="sender">The sender to surface to event handlers.</param>
    /// <param name="e">Event arguments.</param>
    protected internal virtual void OnAttachedToParent(object? sender, EventArgs e) => AttachedToParent?.Invoke(sender, e);

    /// <summary>
    /// Hook invoked when a child is added beneath this object.
    /// </summary>
    /// <param name="child">The child that was added.</param>
    protected internal virtual void OnChildAdded(AnimationObject child) { }

    /// <summary>
    /// Hook invoked when a child is removed from beneath this object.
    /// </summary>
    /// <param name="child">The child that was removed.</param>
    protected internal virtual void OnChildRemoved(AnimationObject child) { }
}
