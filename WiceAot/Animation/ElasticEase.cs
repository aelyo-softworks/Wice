namespace Wice.Animation;

/// <summary>
/// Produces an elastic easing that simulates a spring-like, oscillatory motion,
/// often used to create bouncy animations that overshoot and settle over time.
/// </summary>
/// <remarks>
/// - The easing multiplies a time-dependent amplitude envelope (linear or exponential,
///   depending on <see cref="Springiness"/>) by a sine wave whose frequency is controlled
///   by <see cref="Oscillations"/>.
/// - Negative values for <see cref="Oscillations"/> or <see cref="Springiness"/> are treated
///   as zero at evaluation time; the properties themselves are not clamped.
/// - Typical callers pass <c>normalizedTime</c> within [0, 1]. Values outside this range
///   are extrapolated by the same formula and may overshoot.
/// - Returned values commonly lie in [-1, 1] and may overshoot/undershoot by design.
/// </remarks>
public class ElasticEase : IEasingFunction
{
    /// <summary>
    /// Initializes a new instance of <see cref="ElasticEase"/> with default parameters:
    /// <see cref="Oscillations"/> = 3 and <see cref="Springiness"/> = 3.
    /// </summary>
    public ElasticEase()
    {
        Oscillations = 3;
        Springiness = 3;
    }

    /// <summary>
    /// Gets or sets the number of full oscillations of the sine wave over the input domain [0, 1].
    /// </summary>
    /// <remarks>
    /// Negative values are treated as 0 during evaluation, resulting in no oscillation.
    /// Larger values increase the frequency of the oscillations.
    /// </remarks>
    public int Oscillations { get; set; }

    /// <summary>
    /// Gets or sets the springiness (stiffness) of the motion.
    /// </summary>
    /// <remarks>
    /// - Values less than 0 are treated as 0 during evaluation.
    /// - At 0, the amplitude envelope grows linearly with time.
    /// - At values greater than 0, the amplitude grows exponentially and is normalized
    ///   so that the envelope reaches 1 at <c>normalizedTime = 1</c>.
    /// </remarks>
    public float Springiness { get; set; }

    /// <summary>
    /// Transforms a normalized time value to an elastic, oscillatory eased progress value.
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter, typically in the range [0, 1]. Values outside this range
    /// are extrapolated and may produce larger magnitudes.
    /// </param>
    /// <returns>
    /// The eased progress value. Due to the oscillatory nature, the result may overshoot/undershoot
    /// and is not guaranteed to be within [0, 1].
    /// </returns>
    /// <remarks>
    /// Implementation details:
    /// - Let <c>oscillations = max(0, Oscillations)</c>.
    /// - Let <c>springiness = max(0, Springiness)</c>.
    /// - Compute an envelope:
    ///   - If <c>springiness == 0</c>, <c>expo = normalizedTime</c>.
    ///   - Else, <c>expo = (exp(springiness * t) - 1) / (exp(springiness) - 1)</c>.
    /// - Return <c>expo * sin((2π * oscillations + π/2) * t)</c>.
    /// </remarks>
    public float Ease(float normalizedTime)
    {
        var oscillations = Math.Max(0.0, Oscillations);
        var springiness = Math.Max(0.0, Springiness);

        double expo;
        if (springiness == 0)
        {
            expo = normalizedTime;
        }
        else
        {
            expo = (Math.Exp(springiness * normalizedTime) - 1) / (Math.Exp(springiness) - 1);
        }

        return (float)(expo * Math.Sin((Math.PI * 2 * oscillations + Math.PI * 0.5) * normalizedTime));
    }
}
