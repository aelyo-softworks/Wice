namespace Wice.Animation;

/// <summary>
/// Circular ease-in function (EaseInCirc) implementing <see cref="IEasingFunction"/>.
/// Transforms normalized time t in [0, 1] using: f(t) = 1 - √(1 - t²),
/// producing a slow start that accelerates towards the end.
/// </summary>
public class CircleEase : IEasingFunction
{
    /// <summary>
    /// Evaluates the circular ease-in function.
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter t, typically in [0, 1]. Values outside this range are clamped.
    /// </param>
    /// <returns>
    /// The eased progress value computed as 1 - √(1 - t²), where t is the clamped
    /// <paramref name="normalizedTime"/>.
    /// </returns>
    public float Ease(float normalizedTime)
    {
        normalizedTime = Math.Max(0, Math.Min(1, normalizedTime));
        return (float)(1 - Math.Sqrt(1 - normalizedTime * normalizedTime));
    }
}
