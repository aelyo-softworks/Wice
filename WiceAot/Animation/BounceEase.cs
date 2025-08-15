namespace Wice.Animation;

/// <summary>
/// Easing function that simulates a bouncing motion (like a ball dropped onto a surface),
/// producing a series of decaying parabolic arcs over the normalized time domain [0, 1].
/// </summary>
/// <remarks>
/// - The curve is piecewise parabolic and non-monotonic by design (it "bounces").
/// - <see cref="Bounces"/> controls how many visible rebounds occur.
/// - <see cref="Bounciness"/> controls how quickly bounce amplitudes decay.
/// - The implementation is O(1), allocation-free, and side-effect free with respect to <see cref="Ease(float)"/>.
/// - Callers typically pass <c>normalizedTime</c> in [0, 1]. Values outside this range extrapolate the curve.
/// </remarks>
public class BounceEase : IEasingFunction
{
    /// <summary>
    /// Initializes a new instance of <see cref="BounceEase"/>.
    /// </summary>
    /// <remarks>
    /// Defaults:
    /// - <see cref="Bounces"/> = 3
    /// - <see cref="Bounciness"/> = 2
    /// </remarks>
    public BounceEase()
    {
        Bounces = 3;
        Bounciness = 2;
    }

    /// <summary>
    /// Gets or sets the number of rebound segments to simulate.
    /// </summary>
    /// <value>
    /// Non-negative integer. Higher values produce more bounces with smaller spacing between peaks.
    /// </value>
    public int Bounces { get; set; }

    /// <summary>
    /// Gets or sets the decay factor for successive bounce amplitudes.
    /// </summary>
    /// <value>
    /// A value greater than 1 typically produces visibly decaying bounces. Extremely large values tend to keep
    /// early bounces high while still decaying over time. Internally, the algorithm applies safeguards to avoid
    /// degenerate math in logarithms and divisions.
    /// </value>
    public float Bounciness { get; set; }

    /// <summary>
    /// Transforms a normalized time value in [0, 1] into a bounced progress value in [0, 1].
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter, typically in the range [0, 1]. 0 represents the start of the animation and 1 the end.
    /// </param>
    /// <returns>
    /// The eased progress value exhibiting a bounce profile. The value is 0 at t=0, approaches 1 near t=1,
    /// and oscillates in between according to <see cref="Bounces"/> and <see cref="Bounciness"/>.
    /// </returns>
    /// <remarks>
    /// Implementation details:
    /// - Maps <paramref name="normalizedTime"/> to a geometric "unit" domain derived from a finite geometric series
    ///   controlled by <see cref="Bounciness"/> and <see cref="Bounces"/>.
    /// - Uses a logarithm in the bounciness base to determine the current bounce segment.
    /// - Computes a symmetric parabolic arc over the segment, scaled by a decaying amplitude.
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// var ease = new BounceEase { Bounces = 3, Bounciness = 2.0f };
    /// for (int i = 0; i <= 10; i++)
    /// {
    ///     float t = i / 10f;
    ///     float y = ease.Ease(t);
    ///     // Use y to drive an animation curve.
    /// }
    /// </code>
    /// </example>
    public float Ease(float normalizedTime)
    {
        var bounces = Math.Max(0, Bounces);
        var bounciness = Math.Min(1.001, (double)Bounciness);

        var pow = Math.Pow(bounciness, bounces);
        var oneMinusBounciness = 1 - bounciness;

        var sumOfUnits = (1 - pow) / oneMinusBounciness + pow * 0.5;
        var unitAtT = normalizedTime * sumOfUnits;

        var bounceAtT = Math.Log(-unitAtT * (1 - bounciness) + 1, bounciness);
        var start = Math.Floor(bounceAtT);
        var end = start + 1;

        var startTime = (1 - Math.Pow(bounciness, start)) / (oneMinusBounciness * sumOfUnits);
        var endTime = (1 - Math.Pow(bounciness, end)) / (oneMinusBounciness * sumOfUnits);

        var midTime = (startTime + endTime) * 0.5;
        var timeRelativeToPeak = normalizedTime - midTime;
        var radius = midTime - startTime;
        var amplitude = Math.Pow(1 / bounciness, bounces - start);

        return (float)(-amplitude / (radius * radius) * (timeRelativeToPeak - radius) * (timeRelativeToPeak + radius));
    }
}
