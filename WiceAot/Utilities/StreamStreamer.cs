namespace Wice.Utilities;

/// <summary>
/// Provides a simple <see cref="IReadStreamer"/> implementation that delegates stream acquisition to an event.
/// </summary>
public class StreamStreamer : IReadStreamer
{
    /// <summary>
    /// Occurs when a readable stream is requested.
    /// Handlers may set <see cref="ValueEventArgs{T}.Value"/> to provide a <see cref="System.IO.Stream"/> instance.
    /// </summary>
    public event EventHandler<ValueEventArgs<Stream?>>? GetRead;

    /// <summary>
    /// Raises the <see cref="GetRead"/> event.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data that carries the <see cref="System.IO.Stream"/> to read.</param>
    protected virtual void OnGetRead(object? sender, ValueEventArgs<Stream?> e) => GetRead?.Invoke(sender, e);

    /// <summary>
    /// Gets a readable stream by raising <see cref="GetRead"/> and returning the value supplied by event handlers.
    /// </summary>
    /// <returns>
    /// A readable <see cref="System.IO.Stream"/> provided by an event handler, or <see langword="null"/> if none was supplied.
    /// </returns>
    public Stream? GetReadStream()
    {
        var e = new ValueEventArgs<Stream?>(null, isValueReadOnly: false);
        OnGetRead(this, e);
        return e.Value;
    }
}
