namespace Wice;

/// <summary>
/// Provides event data for load operations that support cancellation,
/// including batching hints and progress information.
/// </summary>
public class LoadEventArgs : CancelEventArgs
{
    /// <summary>
    /// Gets or sets the preferred number of items/events to process in the next batch of the load operation.
    /// </summary>
    public virtual int NextEventBatchSize { get; set; } = 10000;

    /// <summary>
    /// Gets or sets the number of lines/items that have already been loaded when this event is raised.
    /// </summary>
    public virtual int LoadedLines { get; set; }
}
