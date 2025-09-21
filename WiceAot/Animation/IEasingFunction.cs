namespace Wice.Animation;

/// <summary>
/// Defines a contract for easing functions that transform a normalized time value
/// in the range [0, 1] into an eased progress value, typically used by animations
/// to control acceleration and deceleration over time.
/// </summary>
public interface IEasingFunction
{
    /// <summary>
    /// Transforms a normalized time value to an eased progress value.
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter, typically in the range [0, 1], where 0 represents the start
    /// of the animation and 1 represents the end.
    /// </param>
    /// <returns>
    /// The eased progress value corresponding to <paramref name="normalizedTime"/>. Many
    /// implementations return values in [0, 1], though some may overshoot/undershoot.
    /// </returns>
    float Ease(float normalizedTime);
}
