namespace Wice.Animation;

/// <summary>
/// Animates a <see cref="Vector2"/> property from a start (<see cref="From"/>) to an end (<see cref="To"/>) value
/// over the configured <see cref="Animation.Duration"/>. An optional <see cref="IEasingFunction"/> and
/// <see cref="EasingMode"/> can be provided to control the interpolation curve.
/// </summary>
/// <param name="arguments">
/// Construction arguments identifying the target object and property, and the total animation duration.
/// </param>
/// <param name="from">The starting <see cref="Vector2"/> value.</param>
/// <param name="to">The ending <see cref="Vector2"/> value.</param>
/// <param name="easingFunction">The easing function used to transform normalized time to eased progress.</param>
/// <param name="easingMode">The easing mode applied when evaluating <paramref name="easingFunction"/>.</param>
public partial class Vector2PropertyAnimation(PropertyAnimationArguments arguments, Vector2 from, Vector2 to, IEasingFunction? easingFunction = null, EasingMode easingMode = EasingMode.In) : PropertyAnimation(arguments)
{
    /// <summary>
    /// Gets the starting <see cref="Vector2"/> value.
    /// </summary>
    public Vector2 From { get; } = from;

    /// <summary>
    /// Gets the ending <see cref="Vector2"/> value.
    /// </summary>
    public Vector2 To { get; } = to;

    /// <summary>
    /// Gets the easing function used to transform normalized time before interpolation.
    /// </summary>
    public IEasingFunction? EasingFunction { get; } = easingFunction;

    /// <summary>
    /// Gets the easing mode applied when evaluating <see cref="EasingFunction"/>.
    /// </summary>
    public EasingMode EasingMode { get; } = easingMode;

    /// <summary>
    /// Computes the next value to apply to the target property and indicates whether to set it,
    /// continue without setting, or stop.
    /// </summary>
    /// <param name="value">
    /// When the return value is <see cref="AnimationResult.Set"/> or <see cref="AnimationResult.Stop"/>,
    /// contains the <see cref="Vector2"/> value to assign to the target property.
    /// </param>
    /// <returns>
    /// - <see cref="AnimationResult.Stop"/> when the animation is complete or should immediately set the final value.<br/>
    /// - <see cref="AnimationResult.Set"/> to apply the interpolated value for this tick and continue running.<br/>
    /// - <see cref="AnimationResult.Continue"/> is not used by this implementation.
    /// </returns>
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
        var eased = EasingFunction.Ease(normalizedTime, EasingMode);
        value = From + eased * delta;
        //Application.Trace("ElapsedTicks: " + elapsedTicks + " To: " + durationTicks + " " + TargetProperty.Name + " value: " + value);
        return AnimationResult.Set;
    }
}
