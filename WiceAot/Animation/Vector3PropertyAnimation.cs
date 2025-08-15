namespace Wice.Animation;

/// <summary>
/// Animates a <c>Vector3</c>-typed <see cref="BaseObjectProperty"/> on a <see cref="BaseObject"/> over a given duration,
/// optionally applying an easing function.
/// </summary>
/// <remarks>
/// - On each storyboard tick, the animation linearly interpolates between <see cref="From"/> and <see cref="To"/> based on
///   the elapsed time normalized to <c>[0,1]</c>.
/// - When an <see cref="IEasingFunction"/> is provided, the normalized time is transformed before interpolation.
///   This implementation calls <c>Ease(float)</c> on the function; if an overload or extension that accepts
///   an <see cref="EasingMode"/> is available in the host project, it is used via the existing call site.
/// - The animation stops immediately when the start and end values are equal or the duration has elapsed,
///   instructing the caller to set the final value.
/// </remarks>
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

    /// <summary>
    /// Computes the next value to apply to the target property for the current tick.
    /// </summary>
    /// <param name="value">When this method returns with <see cref="AnimationResult.Set"/> or <see cref="AnimationResult.Stop"/>,
    /// contains the interpolated <c>Vector3</c> value.</param>
    /// <returns>
    /// - <see cref="AnimationResult.Stop"/> to set the final value and stop when the duration has elapsed or the start/end are equal.<br/>
    /// - <see cref="AnimationResult.Set"/> to set the interpolated value and continue running.
    /// </returns>
    /// <remarks>
    /// Behavior:
    /// - If <see cref="From"/> equals <see cref="To"/>, the animation requests to set <see cref="To"/> and stop.
    /// - If no <see cref="Storyboard"/> is available, the animation also requests to set <see cref="To"/> and stop.
    /// - Otherwise, the method calculates normalized progress from the storyboard's elapsed ticks over the configured <see cref="Animation.Duration"/>.
    ///   The progress is passed through <see cref="EasingFunction"/> (if present/supported) and used to interpolate the value.
    /// </remarks>
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
