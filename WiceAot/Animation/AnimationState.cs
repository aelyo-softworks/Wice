namespace Wice.Animation;

/// <summary>
/// Represents the lifecycle state of an animation.
/// </summary>
public enum AnimationState
{
    /// <summary>
    /// The animation has not been started.
    /// </summary>
    NotStarted,

    /// <summary>
    /// The animation is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// The animation has been stopped or has completed.
    /// </summary>
    Stopped
}
