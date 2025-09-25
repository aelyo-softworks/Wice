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

    /// <inheritdoc/>
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
        }
    }

    /// <summary>
    /// Attempts to retrieve a value associated with the current animation context.
    /// </summary>
    /// <param name="value">When this method returns, contains the retrieved value if the operation succeeds; otherwise, <see
    /// langword="null"/>.</param>
    /// <returns>An <see cref="AnimationResult"/> indicating the outcome of the operation.  The result specifies whether the
    /// value was successfully retrieved.</returns>
    protected abstract AnimationResult TryGetValue(out object value);
}
