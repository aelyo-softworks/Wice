namespace Wice.Animation;

/// <summary>
/// Base class for animations that drive a single <see cref="BaseObjectProperty"/> on a target <see cref="BaseObject"/>.
/// </summary>
/// <remarks>
/// - Holds a weak reference to the target to avoid keeping it alive during long-running animations.
/// - On each tick (<see cref="OnTick"/>), asks the derived type for the next value via <see cref="TryGetValue(out object)"/> and,
///   depending on the returned <see cref="AnimationResult"/>, updates the target's property and advances the <see cref="Animation.State"/>.
/// - When the property requires writing on the UI thread (<see cref="BaseObjectProperty.Options"/> includes
///   <see cref="BaseObjectPropertyOptions.WriteRequiresMainThread"/>), the write is marshaled to the owning <see cref="Window"/> main thread.
/// </remarks>
/// <seealso cref="Animation"/>
/// <seealso cref="Storyboard"/>
public abstract class PropertyAnimation : Animation
{
    /// <summary>
    /// Raised after the target property's value has been set for a tick.
    /// </summary>
    /// <remarks>
    /// - The <see cref="ValueEventArgs.Value"/> contains the value that was applied to <see cref="TargetProperty"/>.
    /// - This event is fired even when the write is marshaled to the main thread; it is raised on the calling thread after the write request.
    /// </remarks>
    public event EventHandler<ValueEventArgs>? ValueSet;

    /// <summary>
    /// Initializes a new instance of <see cref="PropertyAnimation"/> with the provided arguments.
    /// </summary>
    /// <param name="arguments">Construction arguments that identify the target, property, and duration.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="arguments"/> is invalid (<c>IsValid == false</c>).</exception>
    /// <remarks>
    /// The arguments are effectively frozen by copying the values for <see cref="TargetReference"/>,
    /// <see cref="TargetProperty"/>, and <see cref="AnimationObject.Duration"/> at construction time.
    /// </remarks>
    protected PropertyAnimation(PropertyAnimationArguments arguments)
    {
        ExceptionExtensions.ThrowIfNull(arguments, nameof(arguments));
        if (!arguments.IsValid)
            throw new ArgumentException(null, nameof(arguments));

        // we freeze arguments
        TargetReference = arguments.TargetReference;
        TargetProperty = arguments.TargetProperty;
        Duration = arguments.Duration;
    }

    /// <summary>
    /// Gets the property that this animation drives on the <see cref="Target"/>.
    /// </summary>
    public BaseObjectProperty TargetProperty { get; }

    /// <summary>
    /// Gets a weak reference to the animated <see cref="BaseObject"/> target.
    /// </summary>
    /// <remarks>
    /// A weak reference prevents the animation from prolonging the lifetime of the target object.
    /// </remarks>
    public WeakReference<BaseObject>? TargetReference { get; }

    /// <summary>
    /// Gets the current target instance if it is still alive; otherwise <see langword="null"/>.
    /// </summary>
    public BaseObject? Target
    {
        get
        {
            if (TargetReference == null || !TargetReference.TryGetTarget(out var target))
                return null;

            return target;
        }
    }

    /// <summary>
    /// Raises <see cref="ValueSet"/>. Override to observe or augment the event flow.
    /// </summary>
    /// <param name="sender">The sender propagated to handlers.</param>
    /// <param name="e">Carries the value that was applied to the property.</param>
    protected virtual void OnValueSet(object? sender, ValueEventArgs e) => ValueSet?.Invoke(sender, e);

    /// <inheritdoc />
    public override string ToString() => base.ToString() + " TargetProperty: " + TargetProperty + " Duration: " + Duration + " State: " + State;

    /// <summary>
    /// Advances the animation by one tick.
    /// </summary>
    /// <remarks>
    /// - If the <see cref="Target"/> is no longer available, the animation transitions to <see cref="AnimationState.Stopped"/>.
    /// - Calls <see cref="TryGetValue(out object)"/> to determine the next action:
    ///   - <see cref="AnimationResult.Stop"/>: sets the final value, raises <see cref="ValueSet"/>, and stops.
    ///   - <see cref="AnimationResult.Continue"/>: keeps running but does not set a value this tick.
    ///   - <see cref="AnimationResult.Set"/>: sets the computed value and raises <see cref="ValueSet"/>.
    /// - When <see cref="TargetProperty.Options"/> contains <see cref="BaseObjectPropertyOptions.WriteRequiresMainThread"/>,
    ///   the write is scheduled on <see cref="Window.RunTaskOnMainThread(Action, bool)"/> when not on the main thread.
    /// </remarks>
    protected internal override void OnTick()
    {
        var target = Target;
        if (target == null)
        {
            State = AnimationState.Stopped;
            return;
        }

        var res = TryGetValue(out var value);
        if (res == AnimationResult.Stop)
        {
            SetValue();
            State = AnimationState.Stopped;
            return;
        }

        State = AnimationState.Running;
        if (res == AnimationResult.Continue)
            return;

        // else AnimationResult.Set
        SetValue();

        // Local helper that applies the computed value to the target property,
        // marshaling to the main thread if required by the property's options.
        void SetValue()
        {
            if (target != null)
            {
                var win = Window;
                if (win != null && !win.IsRunningAsMainThread && TargetProperty.Options.HasFlag(BaseObjectPropertyOptions.WriteRequiresMainThread))
                {
                    win.RunTaskOnMainThread(() =>
                    {
                        ((IPropertyOwner)target).SetPropertyValue(TargetProperty, value);
                    });
                }
                else
                {
                    ((IPropertyOwner)target).SetPropertyValue(TargetProperty, value);
                }
                OnValueSet(this, new ValueEventArgs(value));
            }
            //#if DEBUG
            //                Application.Trace("Target: '" + target + "' (" + TargetProperty.Name + ") new value: " + ((IPropertyOwner)target).GetPropertyValue(TargetProperty));
            //#endif
        }
    }

    /// <summary>
    /// Computes the next value to apply and instructs the animation how to proceed.
    /// </summary>
    /// <param name="value">When this method returns with <see cref="AnimationResult.Set"/> or <see cref="AnimationResult.Stop"/>,
    /// contains the value to assign to <see cref="TargetProperty"/>.</param>
    /// <returns>
    /// - <see cref="AnimationResult.Continue"/> to skip setting a value this tick but remain <see cref="AnimationState.Running"/>.<br/>
    /// - <see cref="AnimationResult.Set"/> to set <paramref name="value"/> this tick and remain running.<br/>
    /// - <see cref="AnimationResult.Stop"/> to set <paramref name="value"/> and transition to <see cref="AnimationState.Stopped"/>.
    /// </returns>
    /// <remarks>
    /// Implementations should be deterministic for a given tick and respect <see cref="AnimationObject.Duration"/>
    /// and any easing/timing semantics defined by the derived type.
    /// </remarks>
    protected abstract AnimationResult TryGetValue(out object value);
}
