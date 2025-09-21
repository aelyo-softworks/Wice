﻿namespace Wice.Animation;

/// <summary>
/// Represents a quadratic easing function (commonly known as EaseInQuad) that accelerates
/// from zero velocity using the function f(t) = t².
/// </summary>
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
