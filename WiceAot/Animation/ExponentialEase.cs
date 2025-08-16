namespace Wice.Animation;

/// <summary>
/// Applies an exponential easing function to a normalized time value.
/// Maps t in [0,1] to y = (e^(k·t) - 1) / (e^k - 1), where k is <see cref="Exponent"/>.
/// If k == 0, the mapping is linear (y = t).
/// </summary>
/// <seealso cref="IEasingFunction"/>
public class ExponentialEase : IEasingFunction
{
    /// <summary>
    /// Initializes a new instance with <see cref="Exponent"/> set to 2, producing an ease-in curve.
    /// </summary>
    public ExponentialEase()
    {
        Exponent = 2;
    }

    /// <summary>
    /// Gets or sets the exponential factor k that controls the curvature.
    /// </summary>
    /// <value>
    /// The exponent k. Typical useful range is approximately [-10, 10].
    /// </value>
    /// <remarks>
    /// - k &gt; 0 results in acceleration (ease-in).
    /// - k &lt; 0 results in deceleration (ease-out).
    /// - k = 0 yields a linear mapping.
    /// Large absolute values may lead to numerical instability due to exponentiation.
    /// </remarks>
    public float Exponent { get; set; }

    /// <summary>
    /// Transforms a normalized time value to an exponentially eased progress value.
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter t, typically in [0, 1], where 0 is the start and 1 is the end.
    /// </param>
    /// <returns>
    /// The eased progress value y. For t in [0, 1], y is typically in [0, 1].
    /// </returns>
    /// <remarks>
    /// Uses the mapping y = (e^(k·t) - 1) / (e^k - 1). When k == 0, returns t.
    /// </remarks>
    public float Ease(float normalizedTime)
    {
        var factor = Exponent;
        if (factor == 0)
            return normalizedTime;

        return (float)((Math.Exp(factor * normalizedTime) - 1) / (Math.Exp(factor) - 1));
    }
}
