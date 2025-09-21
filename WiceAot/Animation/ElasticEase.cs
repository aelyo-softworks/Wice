namespace Wice.Animation;

/// <summary>
/// Produces an elastic easing that simulates a spring-like, oscillatory motion,
/// often used to create bouncy animations that overshoot and settle over time.
/// </summary>
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
    public int Oscillations { get; set; }

    /// <summary>
    /// Gets or sets the springiness (stiffness) of the motion.
    /// </summary>
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
