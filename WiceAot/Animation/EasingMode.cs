namespace Wice.Animation;

/// <summary>
/// Defines how an easing function interpolates values over time.
/// </summary>
public enum EasingMode
{
    /// <summary>
    /// Starts slowly and accelerates toward the end of the animation (ease-in).
    /// </summary>
    In,

    /// <summary>
    /// Starts quickly and decelerates toward the end of the animation (ease-out).
    /// </summary>
    Out,

    /// <summary>
    /// Accelerates through the first half and decelerates through the second half (ease-in-out).
    /// </summary>
    InOut,
}
