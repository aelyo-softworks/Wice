namespace Wice.Animation;

/// <summary>
/// Quintic ease-in function that maps a normalized time value <c>t</c> to <c>t^5</c>.
/// Use when an animation should start slowly and accelerate toward the end.
/// </summary>
/// <remarks>
/// Characteristics:
/// - Continuous and monotonically increasing over [0, 1].
/// - For inputs in [0, 1], outputs are also in [0, 1].
/// - Inputs outside [0, 1] are not clamped and are simply raised to the 5th power.
/// </remarks>
/// <example>
/// var easing = new QuinticEase();
/// float t = 0.6f;
/// float progress = easing.Ease(t); // 0.6^5 = 0.07776
/// </example>
/// <seealso cref="IEasingFunction"/>
public class QuinticEase : IEasingFunction
{
    /// <summary>
    /// Transforms a normalized time value using a quintic ease-in curve.
    /// </summary>
    /// <param name="normalizedTime">The interpolation parameter, typically in the range [0, 1].</param>
    /// <returns>The eased progress value computed as <c>normalizedTime^5</c>.</returns>
    public float Ease(float normalizedTime) => normalizedTime * normalizedTime * normalizedTime * normalizedTime * normalizedTime;
}
