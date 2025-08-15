namespace Wice.Animation;

/// <summary>
/// Base type for animation graph objects that participate in a simple parent/child lifecycle.
/// </summary>
/// <remarks>
/// - Exposes <see cref="Parent"/> and notifies via <see cref="AttachedToParent"/> and <see cref="DetachedFromParent"/>.
/// - Container types are expected to set <see cref="Parent"/> and call the provided hooks when children are added/removed.
/// - Overrides should be lightweight and call base to preserve event semantics.
/// </remarks>
public abstract class AnimationObject : BaseObject
{
    /// <summary>
    /// Occurs when this instance has been attached to a <see cref="Parent"/>.
    /// </summary>
    /// <remarks>
    /// Raised by <see cref="OnAttachedToParent(object?, System.EventArgs)"/>.
    /// </remarks>
    public event EventHandler<EventArgs>? AttachedToParent;

    /// <summary>
    /// Occurs when this instance has been detached from its <see cref="Parent"/>.
    /// </summary>
    /// <remarks>
    /// Raised by <see cref="OnDetachedFromParent(object?, System.EventArgs)"/>.
    /// </remarks>
    public event EventHandler<EventArgs>? DetachedFromParent;

    /// <summary>
    /// Gets the logical parent in the animation object tree.
    /// </summary>
    /// <remarks>
    /// This value is set by container/owner code managing the hierarchy.
    /// </remarks>
    public AnimationObject? Parent { get; internal set; }

    /// <summary>
    /// Raises <see cref="DetachedFromParent"/>. Override to observe detachment.
    /// </summary>
    /// <param name="sender">The sender to surface to event handlers.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>Overrides should call the base implementation to preserve event delivery.</remarks>
    protected internal virtual void OnDetachedFromParent(object? sender, EventArgs e) => DetachedFromParent?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="AttachedToParent"/>. Override to observe attachment.
    /// </summary>
    /// <param name="sender">The sender to surface to event handlers.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>Overrides should call the base implementation to preserve event delivery.</remarks>
    protected internal virtual void OnAttachedToParent(object? sender, EventArgs e) => AttachedToParent?.Invoke(sender, e);

    /// <summary>
    /// Hook invoked when a child is added beneath this object.
    /// </summary>
    /// <param name="child">The child that was added.</param>
    /// <remarks>
    /// Default is no-op. Container types should call this when they link a child and set its <see cref="Parent"/>.
    /// </remarks>
    protected internal virtual void OnChildAdded(AnimationObject child) { }

    /// <summary>
    /// Hook invoked when a child is removed from beneath this object.
    /// </summary>
    /// <param name="child">The child that was removed.</param>
    /// <remarks>
    /// Default is no-op. Container types should call this when they unlink a child and clear its <see cref="Parent"/> as appropriate.
    /// </remarks>
    protected internal virtual void OnChildRemoved(AnimationObject child) { }
}
