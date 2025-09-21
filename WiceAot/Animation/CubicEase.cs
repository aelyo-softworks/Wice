namespace Wice.Animation;

/// <summary>
/// Easing function that applies a cubic-in curve to the normalized time.
/// </summary>
public class CubicEase : IEasingFunction
{
    /// <summary>
    /// Transforms a normalized time value using a cubic-in easing curve.
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter, typically in [0, 1], where 0 represents the start
    /// of the animation and 1 represents the end. Values outside [0, 1] are not clamped.
    /// </param>
    /// <returns>
    /// The eased progress computed as <c>normalizedTime ^ 3</c>.
    /// </returns>
    public float Ease(float normalizedTime) => normalizedTime * normalizedTime * normalizedTime;
}
