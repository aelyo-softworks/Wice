namespace Wice.Animation;

/// Easing function that raises the normalized time to a specified non-negative power.
/// </summary>
public class PowerEase : IEasingFunction
{
    /// <summary>
    /// Initializes a new instance of <see cref="PowerEase"/> with <see cref="Power"/> set to 2.
    /// </summary>
    public PowerEase()
    {
        Power = 2;
    }

    /// <summary>
    /// Gets or sets the exponent applied to the normalized time.
    /// </summary>
    /// <value>
    /// The power used in <see cref="Ease(float)"/>. Defaults to 2.
    /// Negative values are treated as 0 during evaluation.
    /// </value>
    public float Power { get; set; }

    /// <summary>
    /// Transforms a normalized time value by raising it to <see cref="Power"/> (clamped to non-negative).
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter, typically in the range [0, 1], where 0 is the start and 1 is the end.
    /// </param>
    /// <returns>
    /// The eased progress value. For non-negative powers and inputs in [0, 1], the result is in [0, 1].
    /// Note: When <see cref="Power"/> is 0, all inputs (including 0) yield 1 due to Math.Pow behavior.
    /// </returns>
    public float Ease(float normalizedTime)
    {
        var power = Math.Max(0, Power);
        return (float)Math.Pow(normalizedTime, power);
    }
}
