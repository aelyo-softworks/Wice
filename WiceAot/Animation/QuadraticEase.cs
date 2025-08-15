namespace Wice.Animation;

/// <summary>
/// Represents a quadratic easing function (commonly known as EaseInQuad) that accelerates
/// from zero velocity using the function f(t) = t².
/// </summary>
/// <remarks>
/// - Input is expected to be a normalized time value in [0, 1].<br/>
/// - For inputs in [0, 1], outputs are also within [0, 1].<br/>
/// - The mapping is continuous and strictly increasing on [0, 1].<br/>
/// - No clamping is performed; values outside [0, 1] will be squared as-is.
/// </remarks>
public class QuadraticEase : IEasingFunction
{
    /// <summary>
    /// Transforms a normalized time value to an eased progress value using f(t) = t².
    /// </summary>
    /// <param name="normalizedTime">
    /// The interpolation parameter, typically in the range [0, 1], where 0 represents the start
    /// of the animation and 1 represents the end.
    /// </param>
    /// <returns>
    /// The eased progress value corresponding to <paramref name="normalizedTime"/>. For inputs in [0, 1],
    /// the result lies within [0, 1] and produces an accelerating curve.
    /// </returns>
    public float Ease(float normalizedTime) => normalizedTime * normalizedTime;
}
