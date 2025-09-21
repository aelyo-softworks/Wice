namespace Wice.Animation;

/// <summary>
/// Easing function that produces a smooth ease-out curve based on a sine wave.
/// Maps normalized time <c>t</c> in [0, 1] to progress <c>p = 1 - cos(π/2 · t)</c>.
/// </summary>
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
    public float Ease(float normalizedTime) => (float)(1 - Math.Sin(Math.PI * 0.5 * (1 - normalizedTime)));
}
