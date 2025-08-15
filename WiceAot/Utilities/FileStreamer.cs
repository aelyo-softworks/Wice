namespace Wice.Utilities;

/// <summary>
/// Provides a file-backed implementation of <see cref="IReadStreamer"/> that opens a read-only stream
/// over the specified file path on each call.
/// </summary>
/// <remarks>
/// - The returned stream is opened with <see cref="System.IO.FileShare.ReadWrite"/> to allow reading while
///   other processes or threads may have the file open for writing.
/// - This type only stores the file path and does not keep a file handle between calls.
/// - The caller owns and must dispose the returned <see cref="System.IO.Stream"/>.
/// </remarks>
public class FileStreamer : IReadStreamer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileStreamer"/> class.
    /// </summary>
    /// <param name="filePath">The relative or absolute path of the file to read.</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
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
    /// <remarks>
    /// - The stream is opened with <see cref="System.IO.FileAccess.Read"/> and <see cref="System.IO.FileShare.ReadWrite"/>.
    /// - The caller is responsible for disposing the returned stream.
    /// - This implementation never returns <see langword="null"/>.
    /// </remarks>
    /// <returns>A readable <see cref="System.IO.Stream"/> for the file.</returns>
    /// <exception cref="System.IO.FileNotFoundException">The file specified by <see cref="FilePath"/> does not exist.</exception>
    /// <exception cref="System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="System.IO.DirectoryNotFoundException">The specified path is invalid.</exception>
    /// <exception cref="System.IO.IOException">An I/O error occurred while opening the file.</exception>
    /// <exception cref="System.NotSupportedException">The <see cref="FilePath"/> is in an invalid format.</exception>
    public virtual Stream GetReadStream() => new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
}
