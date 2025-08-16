namespace Wice.Animation;

/// <summary>
/// Easing function that applies a quartic curve to the input time (f(t) = t^4).
/// </summary>
/// <remarks>
/// - Input is typically a normalized time t in [0, 1].
/// - The mapping starts slowly and accelerates toward the end (ease-in).
/// - Values outside [0, 1] are not clamped; t &gt; 1 will yield values &gt; 1.
/// - For t &lt; 0, the result remains positive due to the even power.
/// </remarks>
/// <seealso cref="IEasingFunction"/>
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
