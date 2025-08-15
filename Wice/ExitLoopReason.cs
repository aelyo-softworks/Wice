namespace Wice;

/// <summary>
/// Represents the reason why a message loop exited.
/// </summary>
/// <remarks>
/// Useful for diagnostics, logging, and post-loop control flow.
/// </remarks>
public enum ExitLoopReason
{
    /// <summary>
    /// A general quit request was received and the loop terminated.
    /// </summary>
    Quit,

    /// <summary>
    /// The application initiated a shutdown causing the loop to exit.
    /// </summary>
    AppQuit,

    /// <summary>
    /// The owning object or context was disposed, forcing the loop to end.
    /// </summary>
    Disposed,

    /// <summary>
    /// A provided function or callback requested the loop to exit.
    /// </summary>
    Func,

    /// <summary>
    /// An unhandled or unexpected message caused the loop to terminate.
    /// </summary>
    UnhandledMessage,
}
