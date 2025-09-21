namespace Wice.Animation;

/// <summary>
/// Animates a single float property from a starting value (<see cref="From"/>) to an ending value (<see cref="To"/>)
/// over the configured <see cref="Animation.Duration"/>, using an <see cref="IEasingFunction"/> and an <see cref="EasingMode"/>.
/// </summary>
// same idea as
// https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/easing-functions
public partial class SinglePropertyAnimation(PropertyAnimationArguments arguments, float from, float to, IEasingFunction? easingFunction = null, EasingMode easingMode = EasingMode.In)
    : PropertyAnimation(arguments)
{
    /// <summary>
    /// Gets the starting value for the interpolation.
    /// </summary>
    public float From { get; } = from;

    /// <summary>
    /// Gets the ending value for the interpolation.
    /// </summary>
    public float To { get; } = to;

    /// <summary>
    /// Gets the easing function used to transform normalized time into eased progress.
    /// </summary>
    public IEasingFunction? EasingFunction { get; } = easingFunction;

    /// <summary>
    /// Gets the easing mode that controls how the <see cref="EasingFunction"/> is applied over time.
    /// </summary>
    public EasingMode EasingMode { get; } = easingMode;

    /// <summary>
    /// Computes the next value to apply to the target property and indicates how the animation should proceed.
    /// </summary>
    /// <param name="value">
    /// When returning <see cref="AnimationResult.Set"/> or <see cref="AnimationResult.Stop"/>, contains the value
    /// to assign to the target property this tick.
    /// </param>
    /// <returns>
    /// - <see cref="AnimationResult.Stop"/> when the animation is complete or trivially resolved (e.g., From == To or elapsed >= duration).<br/>
    /// - <see cref="AnimationResult.Set"/> when an interpolated value should be written this tick.<br/>
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
