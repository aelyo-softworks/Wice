namespace Wice.Animation;

/// <summary>
/// An easing function that creates a "back" motion: it initially overshoots in the
/// opposite direction before accelerating toward the target.
/// </summary>
public class BackEase : IEasingFunction
{
    /// <summary>
    /// Creates a new <see cref="BackEase"/> with a default <see cref="Amplitude"/> of <c>1</c>.
    /// </summary>
    public BackEase()
    {
        Amplitude = 1;
    }

    /// <summary>
    /// Controls the intensity of the "back" overshoot effect.
    /// </summary>
    /// <value>
    /// A non-negative value where higher values increase the overshoot. Negative inputs are
    /// coerced to <c>0</c> at evaluation time.
    /// </value>
    public float Amplitude { get; set; }

    /// <summary>
    /// Transforms a normalized time value to an eased progress value using the Back easing curve.
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter, typically in the range [0, 1], where 0 represents the start
    /// and 1 represents the end of the animation.
    /// </param>
    /// <returns>
    /// The eased progress value. Values may overshoot/undershoot outside [0, 1] depending on
    /// <see cref="Amplitude"/>. Inputs outside [0, 1] are processed as provided.
    /// </returns>
    public float Ease(float normalizedTime)
    {
        var amp = Math.Max(0, Amplitude);
        return (float)(Math.Pow(normalizedTime, 3) - normalizedTime * amp * Math.Sin(Math.PI * normalizedTime));
    }
}
