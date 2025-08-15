namespace Wice.Animation;

/// <summary>
/// Defines how an easing function interpolates values over time.
/// </summary>
/// <remarks>
/// - In: starts slowly and accelerates (ease-in).
/// - Out: starts quickly and decelerates (ease-out).
/// - InOut: accelerates in the first half and decelerates in the second half (ease-in-out).
/// </remarks>
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
