namespace Wice.Animation;

/// <summary>
/// Quintic ease-in function that maps a normalized time value <c>t</c> to <c>t^5</c>.
/// Use when an animation should start slowly and accelerate toward the end.
/// </summary>
public class QuinticEase : IEasingFunction
{
    /// <summary>
    /// Transforms a normalized time value using a quintic ease-in curve.
    /// </summary>
    /// <param name="normalizedTime">The interpolation parameter, typically in the range [0, 1].</param>
    /// <returns>The eased progress value computed as <c>normalizedTime^5</c>.</returns>
    public float Ease(float normalizedTime) => normalizedTime * normalizedTime * normalizedTime * normalizedTime * normalizedTime;
}
