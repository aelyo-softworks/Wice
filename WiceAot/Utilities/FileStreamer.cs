namespace Wice.Utilities;

/// <summary>
/// Provides a file-backed implementation of <see cref="IReadStreamer"/> that opens a read-only stream
/// over the specified file path on each call.
/// </summary>
public class FileStreamer : IReadStreamer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileStreamer"/> class.
    /// </summary>
    /// <param name="filePath">The relative or absolute path of the file to read.</param>
    public FileStreamer(string filePath)
    {
        ExceptionExtensions.ThrowIfNull(filePath, nameof(filePath));
        FilePath = filePath;
    }

    /// <summary>
    /// Gets the file system path that this streamer will read from.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Returns the configured file path.
    /// </summary>
    /// <returns>The value of <see cref="FilePath"/>.</returns>
    public override string ToString() => FilePath;

    /// <summary>
    /// Opens a readable stream for the configured file.
    /// </summary>
    /// <returns>A readable <see cref="System.IO.Stream"/> for the file.</returns>
    public virtual Stream GetReadStream() => new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
}
