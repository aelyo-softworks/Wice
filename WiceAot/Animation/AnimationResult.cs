namespace Wice.Animation;

/// <summary>
/// Represents the outcome of processing a single animation step (tick).
/// </summary>
/// <remarks>
/// This value is typically returned by animation evaluators to control if the animation
/// should keep running, finish immediately, or just set a final value.
/// </remarks>
public enum AnimationResult
{
    /// <summary>
    /// Apply the target value once and complete immediately (one-shot set).
    /// </summary>
    Set,

    /// <summary>
    /// The animation should remain active and be evaluated on the next tick/frame.
    /// </summary>
    Continue,

    /// <summary>
    /// The animation has finished and should be stopped; no further ticks are required.
    /// </summary>
    Stop
}
