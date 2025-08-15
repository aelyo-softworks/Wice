namespace Wice;

/// <summary>
/// Provides event data for load operations that support cancellation,
/// including batching hints and progress information.
/// </summary>
/// <remarks>
/// Set <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to <c>true</c>
/// within an event handler to cancel the ongoing load operation.
/// </remarks>
/// <seealso cref="System.ComponentModel.CancelEventArgs"/>
public class LoadEventArgs : CancelEventArgs
{
    /// <summary>
    /// Gets or sets the preferred number of items/events to process in the next batch of the load operation.
    /// </summary>
    /// <remarks>
    /// This value is a hint to the producer about how many items to enqueue or process next.
    /// Defaults to <c>10,000</c>. Implementations may clamp or ignore non-positive values.
    /// </remarks>
    public virtual int NextEventBatchSize { get; set; } = 10000;

    /// <summary>
    /// Gets or sets the number of lines/items that have already been loaded when this event is raised.
    /// </summary>
    /// <remarks>
    /// Can be used by handlers to report progress or adjust subsequent processing.
    /// </remarks>
    public virtual int LoadedLines { get; set; }
}
