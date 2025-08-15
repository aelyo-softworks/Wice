namespace Wice.Animation;

/// <summary>
/// Base abstraction for a time-based animation unit.
/// </summary>
/// <remarks>
/// - Lives under a <see cref="Storyboard"/> either directly or through an <see cref="AnimationGroup"/>.
/// - Exposes a simple state machine via <see cref="State"/> and raises <see cref="StateChanged"/> on transitions.
/// - Derived types implement <see cref="OnTick"/> to progress the animation on each storyboard tick.
/// </remarks>
/// <seealso cref="Storyboard"/>
/// <seealso cref="AnimationGroup"/>
public abstract class Animation : AnimationObject
{
    private AnimationState _state;

    /// <summary>
    /// Occurs when <see cref="State"/> changes.
    /// </summary>
    /// <remarks>
    /// The <see cref="ValueEventArgs.Value"/> carries the new <see cref="AnimationState"/> value.
    /// </remarks>
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
    /// <remarks>
    /// Resolution logic:
    /// - If <see cref="AnimationObject.Parent"/> is a <see cref="Storyboard"/>, that instance is returned.
    /// - If the parent is an <see cref="AnimationGroup"/>, returns the group's <see cref="Animation.AnimationGroup.Storyboard"/>.
    /// - Otherwise, returns null.
    /// </remarks>
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
    /// <remarks>
    /// Derived classes typically set this in their constructor or when configured.
    /// </remarks>
    public TimeSpan Duration { get; protected set; }

    /// <summary>
    /// Gets the current <see cref="AnimationState"/> of this animation.
    /// </summary>
    /// <remarks>
    /// Setting the state (by derived types) raises <see cref="StateChanged"/> when it actually changes.
    /// </remarks>
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
    /// <remarks>
    /// Override to observe state changes. Call the base implementation to preserve event delivery.
    /// </remarks>
    protected virtual void OnStateChanged(object sender, ValueEventArgs e) => StateChanged?.Invoke(sender, e);

    /// <summary>
    /// Advances the animation one tick. Called by the owning <see cref="Storyboard"/>.
    /// </summary>
    /// <remarks>
    /// Implementations should compute their current progress/time based on the storyboard clock
    /// and update their targets accordingly.
    /// </remarks>
    protected internal abstract void OnTick();
}
