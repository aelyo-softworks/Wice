namespace Wice;

/// <summary>
/// Provides a mechanism to obtain a readable <see cref="System.IO.Stream"/> instance.
/// </summary>
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
