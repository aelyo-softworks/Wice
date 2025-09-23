namespace Wice.Animation;

/// <summary>
/// Animates a <c>Vector3</c>-typed <see cref="BaseObjectProperty"/> on a <see cref="BaseObject"/> over a given duration,
/// optionally applying an easing function.
/// </summary>
public partial class Vector3PropertyAnimation(
    PropertyAnimationArguments arguments,
    Vector3 from,
    Vector3 to,
    IEasingFunction? easingFunction = null,
    EasingMode easingMode = EasingMode.In)
    : PropertyAnimation(arguments)
{
    /// <summary>
    /// Gets the start value for the animation.
    /// </summary>
    public Vector3 From { get; } = from;

    /// <summary>
    /// Gets the end value for the animation.
    /// </summary>
    public Vector3 To { get; } = to;

    /// <summary>
    /// Gets the easing function applied to the normalized time prior to interpolation.
    /// When <see langword="null"/>, interpolation is linear.
    /// </summary>
    public IEasingFunction? EasingFunction { get; } = easingFunction;

    /// <summary>
    /// Gets the easing mode to use with the <see cref="EasingFunction"/> when supported by the implementation/extension.
    /// </summary>
    public EasingMode EasingMode { get; } = easingMode;

    /// <inheritdoc/>
    protected override AnimationResult TryGetValue(out object value)
    {
        // special "set" case
        if (From == To)
        {
            value = To;
            return AnimationResult.Stop;
        }

        if (Storyboard == null)
        {
            value = To;
            return AnimationResult.Stop;
        }

        var watch = Storyboard.Watch;
        var elapsedTicks = watch.ElapsedTicks;
        var durationTicks = Duration.Ticks;
        if (elapsedTicks >= durationTicks)
        {
            value = To;
            //Application.Trace("ElapsedTicks: " + elapsedTicks + " To: " + durationTicks + " " + TargetPropertyName + " stop at " + value);
            return AnimationResult.Stop;
        }

        var delta = To - From;
        var normalizedTime = elapsedTicks / (float)durationTicks;

        // Note: In this codebase an overload/extension Ease(float, EasingMode) may be available.
        // If not, consumers should ensure EasingFunction.Ease(float) is used or EasingFunction is non-null with a compatible signature.
        var eased = EasingFunction.Ease(normalizedTime, EasingMode);

        value = From + eased * delta;
        //Application.Trace("ElapsedTicks: " + elapsedTicks + " To: " + durationTicks + " " + TargetProperty.Name + " value: " + value);
        return AnimationResult.Set;
    }
}
