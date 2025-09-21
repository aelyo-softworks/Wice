namespace Wice.Animation;

/// <summary>
/// Base abstraction for a time-based animation unit.
/// </summary>
public abstract class Animation : AnimationObject
{
    private AnimationState _state;

    /// <summary>
    /// Occurs when <see cref="State"/> changes.
    /// </summary>
    public event EventHandler<ValueEventArgs>? StateChanged;

    /// <summary>
    /// Gets the owning <see cref="AnimationGroup"/> if this animation is contained within one; otherwise null.
    /// </summary>
    public AnimationGroup? AnimationGroup => Parent as AnimationGroup;

    /// <summary>
    /// Gets the <see cref="Window"/> associated with the resolved <see cref="Storyboard"/>, if any.
    /// </summary>
    public Window? Window => Storyboard?.Window;

    /// <summary>
    /// Gets the effective <see cref="Storyboard"/> that drives this animation.
    /// </summary>
    public Storyboard? Storyboard
    {
        get
        {
            if (Parent is Storyboard sb)
                return sb;

            if (Parent is AnimationGroup group)
                return group.Storyboard;

            return null;
        }
    }

    /// <summary>
    /// Gets the total duration of this animation.
    /// </summary>
    public TimeSpan Duration { get; protected set; }

    /// <summary>
    /// Gets the current <see cref="AnimationState"/> of this animation.
    /// </summary>
    public AnimationState State
    {
        get => _state;
        protected set
        {
            if (_state == value)
                return;

            _state = value;
            OnStateChanged(this, new ValueEventArgs(_state));
        }
    }

    /// <summary>
    /// Raises the <see cref="StateChanged"/> event.
    /// </summary>
    /// <param name="sender">The sender to surface to event handlers.</param>
    /// <param name="e">The event arguments carrying the new state.</param>
    protected virtual void OnStateChanged(object sender, ValueEventArgs e) => StateChanged?.Invoke(sender, e);

    /// <summary>
    /// Advances the animation one tick. Called by the owning <see cref="Storyboard"/>.
    /// </summary>
    protected internal abstract void OnTick();
}
