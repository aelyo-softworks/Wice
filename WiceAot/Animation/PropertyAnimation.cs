namespace Wice.Animation;

/// <summary>
/// Base class for animations that drive a single <see cref="BaseObjectProperty"/> on a target <see cref="BaseObject"/>.
/// </summary>
public abstract class PropertyAnimation : Animation
{
    /// <summary>
    /// Raised after the target property's value has been set for a tick.
    /// </summary>
    public event EventHandler<ValueEventArgs>? ValueSet;

    /// <summary>
    /// Initializes a new instance of <see cref="PropertyAnimation"/> with the provided arguments.
    /// </summary>
    /// <param name="arguments">Construction arguments that identify the target, property, and duration.</param>
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
    protected abstract AnimationResult TryGetValue(out object value);
}
