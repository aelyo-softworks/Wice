namespace Wice.Animation;

/// <summary>
/// Easing function that applies a cubic-in curve to the normalized time.
/// </summary>
/// <remarks>
/// This function implements <c>f(t) = t^3</c>, producing a smooth acceleration
/// from rest. It is continuous and strictly increasing over the real numbers,
/// maps 0 to 0 and 1 to 1, and does not overshoot within the [0, 1] interval.
/// Values outside [0, 1] are not clamped and will be cubed as-is (e.g., negative
/// inputs yield negative outputs; inputs greater than 1 grow faster than linear).
/// </remarks>
/// <seealso cref="IEasingFunction"/>
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
