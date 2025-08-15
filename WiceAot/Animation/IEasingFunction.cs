namespace Wice.Animation;

/// <summary>
/// Defines a contract for easing functions that transform a normalized time value
/// in the range [0, 1] into an eased progress value, typically used by animations
/// to control acceleration and deceleration over time.
/// </summary>
/// <remarks>
/// Implementations should be pure and side-effect free. Callers typically pass
/// <c>normalizedTime</c> values within [0, 1]. An easing function commonly
/// returns values in [0, 1], but specific implementations may overshoot or
/// undershoot (e.g., back/elastic easings) by design.
/// 
/// For smooth animations, prefer continuous and monotonic mappings unless a
/// special effect is desired.
/// </remarks>
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
