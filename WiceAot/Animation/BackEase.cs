namespace Wice.Animation;

/// <summary>
/// An easing function that creates a "back" motion: it initially overshoots in the
/// opposite direction before accelerating toward the target.
/// </summary>
/// <remarks>
/// This function is commonly used to add a subtle anticipation effect to animations.
/// The implementation is:
/// <code>
/// f(t) = t^3 - t * A * sin(πt)
/// </code>
/// where <c>t</c> is <paramref name="normalizedTime"/> and <c>A</c> is <see cref="Amplitude"/>.
/// Negative amplitude values are treated as <c>0</c>. The function may overshoot/undershoot
/// for certain amplitude values by design.
/// </remarks>
/// <example>
/// Example usage:
/// <code language="csharp">
/// IEasingFunction easing = new BackEase { Amplitude = 1.2f };
/// float t = 0.5f; // normalized time in [0, 1]
/// float eased = easing.Ease(t);
/// </code>
/// </example>
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
    /// <remarks>
    /// Typical values are in the range [0, 3]. Setting this to <c>0</c> reduces the effect,
    /// approaching a smooth cubic curve without the back-overshoot term.
    /// </remarks>
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
    /// <remarks>
    /// The amplitude is clamped to be non-negative at evaluation time.
    /// </remarks>
    public float Ease(float normalizedTime)
    {
        var amp = Math.Max(0, Amplitude);
        return (float)(Math.Pow(normalizedTime, 3) - normalizedTime * amp * Math.Sin(Math.PI * normalizedTime));
    }
}
