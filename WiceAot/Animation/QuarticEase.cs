namespace Wice.Animation;

/// <summary>
/// Easing function that applies a quartic curve to the input time (f(t) = t^4).
/// </summary>
public class QuarticEase : IEasingFunction
{
    /// <summary>
    /// Transforms a normalized time value using a quartic curve: f(t) = t^4.
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter t, typically in [0, 1], where 0 is the start
    /// of the animation and 1 is the end.
    /// </param>
    /// <returns>
    /// The eased progress value f(t) = t^4. This function does not clamp the input.
    /// </returns>
    public float Ease(float normalizedTime) => normalizedTime * normalizedTime * normalizedTime * normalizedTime;
}
