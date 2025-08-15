namespace Wice.Animation;

/// <summary>
/// Easing function that produces a smooth ease-out curve based on a sine wave.
/// Maps normalized time <c>t</c> in [0, 1] to progress <c>p = 1 - cos(π/2 · t)</c>.
/// </summary>
/// <remarks>
/// This implementation is continuous and monotonic over [0, 1], returning values in [0, 1].
/// Values of <paramref name="normalizedTime"/> outside [0, 1] are not clamped and may
/// yield results outside [0, 1]. Implementation uses the equivalent form
/// <c>1 - sin(π/2 · (1 - t))</c>.
/// </remarks>
public class SineEase : IEasingFunction
{
    /// <summary>
    /// Transforms a normalized time value into eased progress using a sine ease-out curve.
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter, typically in the range [0, 1], where 0 is the start and 1 is the end.
    /// </param>
    /// <returns>
    /// The eased progress value corresponding to <paramref name="normalizedTime"/>.
    /// </returns>
    /// <remarks>
    /// For <c>t ∈ [0,1]</c>, evaluates to <c>1 - cos(π/2 · t)</c>, yielding 0 at t=0 and 1 at t=1.
    /// </remarks>
    public float Ease(float normalizedTime) => (float)(1 - Math.Sin(Math.PI * 0.5 * (1 - normalizedTime)));
}
