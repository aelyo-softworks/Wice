namespace Wice;

/// <summary>
/// Provides a mechanism to obtain a readable <see cref="System.IO.Stream"/> instance.
/// </summary>
/// <remarks>
/// Implementations should return a stream that supports reading, or <see langword="null"/> when no data is available.
/// Callers are responsible for disposing the returned stream when finished.
/// </remarks>
public interface IReadStreamer
{
    /// <summary>
    /// Gets a readable stream for consuming data.
    /// </summary>
    /// <returns>
    /// A readable <see cref="System.IO.Stream"/> instance, or <see langword="null"/> if no stream is available.
    /// The caller owns the returned stream and should dispose it when no longer needed.
    /// </returns>
    Stream? GetReadStream();
}
